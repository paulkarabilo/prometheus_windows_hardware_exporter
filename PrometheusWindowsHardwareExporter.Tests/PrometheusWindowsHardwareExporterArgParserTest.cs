namespace PrometheusWindowsHardwareExporter.Tests;

using JsonSerializer = System.Text.Json.JsonSerializer;
public class PrometheusWindowsHardwareExporterRendererTest
{
    [Fact]
    public void TestDefaultArgs()
    {
        string[] args = Array.Empty<string>();
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal("http://localhost:9182/", config.ListenAddress);
        Assert.Equal(15, config.CollectInterval);
        Assert.Equal("/metrics", config.MetricsPath);
        Assert.Equal(new string[] { "cpu", "gpu", "memory", "motherboard", "storage", "psu", "battery", "network" }, config.Collectors);
        Assert.False(config.Service);
        Assert.False(config.ShowHelp);
    }

    [Fact]
    public void TestCustomListenAddress()
    {
        string[] args = new string[] { "--listen-address=http://localhost:9090" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal("http://localhost:9090", config.ListenAddress);
    }

    [Fact]
    public void TestCustomCollectInterval()
    {
        string[] args = new string[] { "--collect-interval=30" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal(30, config.CollectInterval);
    }

    [Fact]
    public void TestCustomMetricsPath()
    {
        string[] args = new string[] { "--metrics-path=/custom-metrics" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal("/custom-metrics", config.MetricsPath);
    }

    [Fact]
    public void TestCustomCollectors()
    {
        string[] args = new string[] { "--collectors=cpu,memory" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal(new string[] { "cpu", "memory" }, config.Collectors);
    }

    [Fact]
    public void TestServiceFlag()
    {
        string[] args = new string[] { "--service" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.True(config.Service);
    }

    [Fact]
    public void TestHelpFlag()
    {
        string[] args = new string[] { "--help" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.True(config.ShowHelp);
    }

    [Fact]
    public void TestInvalidCollectInterval()
    {
        string[] args = new string[] { "--collect-interval=invalid-value" };
        Assert.Equal(15, PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args).CollectInterval);
    }

    [Fact]
    public void TestJsonConfigParsing()
    {
        string jsonConfig = @"
        {
            ""listen_address"": ""http://localhost:8080"",
            ""collect_interval"": 20,
            ""metrics_path"": ""/custom-metrics"",
            ""collectors"": [""cpu"", ""memory""],
            ""service"": true
        }";

        var config = JsonSerializer.Deserialize<PrometheusWindowsHardwareExporter.Config>(jsonConfig, PrometheusWindowsHardwareExporter.ConfigGenerationContext.Default.Config);

        Assert.Equal("http://localhost:8080", config?.ListenAddress);
        Assert.Equal(20, config?.CollectInterval);
        Assert.Equal("/custom-metrics", config?.MetricsPath);
        Assert.Equal(new string[] { "cpu", "memory" }, config?.Collectors);
        Assert.True(config?.Service ?? false);
    }
}
