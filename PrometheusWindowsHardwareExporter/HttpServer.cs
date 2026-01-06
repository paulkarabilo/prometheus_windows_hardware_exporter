using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrometheusWindowsHardwareExporter
{
    public sealed class HttpServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly SemaphoreSlim _concurrency;
        private readonly TimeSpan _requestTimeout;
        private readonly string _metricsPath;
        private readonly Func<string> _metricsProviderFn;

        public HttpServer(
            string prefix,
            string metricsPath,
            Func<string> metricsProviderFn,
            int maxConcurrentRequests = 8,
            TimeSpan? requestTimeout = null)
        {
            if (!prefix.EndsWith("/"))
                throw new ArgumentException("prefix must end with '/'");
            _metricsPath = metricsPath.StartsWith("/") ? metricsPath : "/" + metricsPath;
            _metricsProviderFn = metricsProviderFn;
            _requestTimeout = requestTimeout ?? TimeSpan.FromSeconds(5);
            _concurrency = new SemaphoreSlim(maxConcurrentRequests);
            _listener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            _listener.Start();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var reg = cancellationToken.Register(() =>
            {
                try
                {
                    _listener.Stop();
                }
                catch { }
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext? context = null;
                try
                {
                    context = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    continue;
                }

                if (!await _concurrency.WaitAsync(0, cancellationToken).ConfigureAwait(false))
                {
                    _ = RespondBusyAsync(context);
                    continue;
                }
                _ = HandleAsync(context, cancellationToken);
            }
        }

        private async Task HandleAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_requestTimeout);
                if (!context.Request.HttpMethod.Equals("GET", StringComparison.Ordinal))
                {
                    await Write(context, 405, "text/plain; charset=utf-8", "method not allowed\n", cts.Token).ConfigureAwait(false);
                    return;
                }

                string path = context.Request.Url?.AbsolutePath ?? "/";

                if (path.Equals(_metricsPath, StringComparison.Ordinal))
                {
                    string metricsText = _metricsProviderFn();
                    await Write(context, 200, "text/plain; charset=utf-8", metricsText, cts.Token).ConfigureAwait(false);
                    return;
                }
                if (path.Equals("/-/health", StringComparison.Ordinal))
                {
                    await Write(context, 200, "text/plain; charset=utf-8", "ok\n", cts.Token).ConfigureAwait(false);
                    return;
                }
                await Write(context, 404, "text/plain; charset=utf-8", "not found\n", cts.Token).ConfigureAwait(false);

            }
            catch { }
            finally
            {
                try { context.Response.Close(); }
                catch { }
                _concurrency.Release();
            }
        }

        private static async Task RespondBusyAsync(HttpListenerContext context)
        {
            try
            {
                await Write(context, 503, "text/plain; charset=utf-8", "busy\n", CancellationToken.None).ConfigureAwait(false);
            }
            catch { }
            finally
            {
                try { context.Response.Close(); }
                catch { }
            }
        }

        private static async Task Write(HttpListenerContext context, int statusCode, string contentType, string content, CancellationToken cancellationToken)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            HttpListenerResponse response = context.Response;

            response.StatusCode = statusCode;
            response.ContentType = contentType;
            response.ContentLength64 = bytes.Length;
            response.Headers["Connection"] = "close";

            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
