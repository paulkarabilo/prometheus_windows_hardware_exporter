namespace PrometheusWindowsHardwareExporter.Tests;

using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class HttpServerTest
{
    [Fact]
    public async Task TestStartAndRespondsToMetricsAndHealth()
    {
        int port = GetFreePort();
        var server = new HttpServer($"http://localhost:{port}/", "/metrics", () => "custom-metrics\n");
        server.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Task runTask = server.RunAsync(cts.Token);

        using var client = new HttpClient();

        string metrics = await client.GetStringAsync($"http://localhost:{port}/metrics");
        Assert.Equal("custom-metrics\n", metrics);

        string health = await client.GetStringAsync($"http://localhost:{port}/-/health");
        Assert.Equal("ok\n", health);

        cts.Cancel();
        await runTask;
    }

    [Fact]
    public void TestThrowsWhenPrefixMissingTrailingSlash()
    {
        Assert.Throws<ArgumentException>(() => new HttpServer("http://localhost:9182", "/metrics", () => "unused"));
    }

    [Fact]
    public async Task TestReturnsMethodNotAllowed()
    {
        int port = GetFreePort();
        var server = new HttpServer($"http://localhost:{port}/", "/metrics", () => "custom-metrics\n");
        server.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Task runTask = server.RunAsync(cts.Token);

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/metrics");
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

        cts.Cancel();
        await runTask;
    }

    [Fact]
    public async Task TestReturnsNotFound()
    {
        int port = GetFreePort();
        var server = new HttpServer($"http://localhost:{port}/", "/metrics", () => "custom-metrics\n");
        server.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Task runTask = server.RunAsync(cts.Token);

        using var client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync($"http://localhost:{port}/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("not found\n", await response.Content.ReadAsStringAsync());

        cts.Cancel();
        await runTask;
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
