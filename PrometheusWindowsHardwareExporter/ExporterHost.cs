using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrometheusWindowsHardwareExporter
{
    internal class ExporterHost : IDisposable
    {
        private readonly Config _config;
        private readonly CachedMetrics _metrics;
        private readonly HttpServer _server;

        public ExporterHost(Config config, CachedMetrics metrics)
        {
            _config = config;
            _metrics = metrics;
            _server = new HttpServer(_config.ListenAddress, _config.MetricsPath, _metrics.GetMetricsText, _config.MaxConcurrent, _config.RequestTimeout);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _server.Start();
                await _server.RunAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ExporterHost encountered an error: {ex}");
                throw;
            }
        }

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
