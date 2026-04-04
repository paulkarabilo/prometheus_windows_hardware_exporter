using LibreHardwareMonitor.Hardware;
using Moq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace PrometheusWindowsHardwareExporter.Tests;

public class ExporterHostTest
{
    [Fact]
    public async Task TestServeMetricsRequest()
    {
        int port = GetFreePort();
        var config = CreateConfig(port, "/metrics");
        var cachedMetrics = CreateCachedMetrics();
        var exporterHost = new ExporterHost(config, cachedMetrics);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Task runTask = exporterHost.RunAsync(cts.Token);

        string requestUrl = config.Web.ListenAddress.TrimEnd('/') + config.Web.MetricsPath;
        using var httpClient = new HttpClient();

        HttpResponseMessage? response = null;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                response = await httpClient.GetAsync(requestUrl, cts.Token);
                break;
            }
            catch (HttpRequestException)
            {
                await Task.Delay(100, cts.Token);
            }
        }

        Assert.NotNull(response);
        Assert.True(response!.IsSuccessStatusCode);
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        string body = await response.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("prometheus_windows_hardware_exporter_up 1", body);

        cts.Cancel();
        await runTask;
    }

    [Fact]
    public async Task TestStopWhenCancellationRequested()
    {
        int port = GetFreePort();
        var config = CreateConfig(port, "/metrics");
        var cachedMetrics = CreateCachedMetrics();
        var exporterHost = new ExporterHost(config, cachedMetrics);

        using var cts = new CancellationTokenSource();
        Task runTask = exporterHost.RunAsync(cts.Token);

        await Task.Delay(200);
        cts.Cancel();

        await runTask;
        Assert.True(runTask.IsCompletedSuccessfully);
    }

    private static Config CreateConfig(int port, string metricsPath)
    {
        return new Config
        {
            Web = new WebConfig
            {
                ListenAddress = $"http://localhost:{port}/",
                MetricsPath = metricsPath
            },
            MaxConcurrent = 2,
            RequestTimeout = TimeSpan.FromSeconds(2)
        };
    }

    private static CachedMetrics CreateCachedMetrics()
    {
        var computerMock = new Mock<IComputer>();
        var hardwareMock = new Mock<IHardware>();
        var sensorMock = new Mock<ISensor>();

        hardwareMock.Setup(h => h.Name).Returns("TestHardware");
        hardwareMock.Setup(h => h.HardwareType).Returns(HardwareType.Cpu);
        hardwareMock.Setup(h => h.Sensors).Returns(new[] { sensorMock.Object });
        hardwareMock.Setup(h => h.SubHardware).Returns(Array.Empty<IHardware>());

        sensorMock.Setup(s => s.Name).Returns("Test Sensor");
        sensorMock.Setup(s => s.Value).Returns(42.0f);
        sensorMock.Setup(s => s.SensorType).Returns(SensorType.Temperature);

        computerMock.Setup(c => c.Hardware).Returns(new[] { hardwareMock.Object });

        return new CachedMetrics(computerMock.Object, TimeSpan.FromSeconds(10));
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
