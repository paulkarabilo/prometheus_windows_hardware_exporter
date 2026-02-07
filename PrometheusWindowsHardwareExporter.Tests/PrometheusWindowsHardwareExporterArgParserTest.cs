namespace PrometheusWindowsHardwareExporter.Tests;

using JsonSerializer = System.Text.Json.JsonSerializer;
public class PrometheusWindowsHardwareExporterRendererTest
{
    [Fact]
    public void TestDefaultArgs()
    {
        string[] args = Array.Empty<string>();
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal("http://localhost:9182/", config.Web.ListenAddress);
        Assert.Equal(15, config.CollectInterval);
        Assert.Equal("/metrics", config.Web.MetricsPath);
        Assert.Equal(new string[] { "cpu", "gpu", "memory", "motherboard", "storage", "psu", "battery", "network" }, config.Collectors.Enabled);
        Assert.False(config.Service);
        Assert.False(config.ShowHelp);
    }

    [Fact]
    public void TestCustomListenAddress()
    {
        string[] args = new string[] { "--web.listen-address=http://localhost:9090" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal("http://localhost:9090", config.Web.ListenAddress);
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
        Assert.Equal("/custom-metrics", config.Web.MetricsPath);
    }

    [Fact]
    public void TestCustomCollectors()
    {
        string[] args = new string[] { "--collectors=cpu,memory" };
        var config = PrometheusWindowsHardwareExporter.ArgsParser.ParseArgs(args);
        Assert.Equal(new string[] { "cpu", "memory" }, config.Collectors.Enabled);
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
    public void TestYamlConfigParsing()
    {

        var args = new string[] { $"--config={Environment.CurrentDirectory}/Resources/test_config.yml" };
        var config = ArgsParser.ParseArgs(args);

        Assert.Equal("http://localhost:8080", config?.Web.ListenAddress);
        Assert.Equal(20, config?.CollectInterval);
        Assert.Equal("/custom-metrics", config?.Web.MetricsPath);
        Assert.Equal(new string[] { "cpu", "memory" }, config?.Collectors.Enabled);
        Assert.True(config?.Service ?? false);
    }
}
