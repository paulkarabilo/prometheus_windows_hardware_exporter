using LibreHardwareMonitor.Hardware;
using Moq;

namespace PrometheusWindowsHardwareExporter.Tests;

public class CachedMetricsTest
{

    [Fact]
    public void TestEmptyCachedMetrics()
    {
        var computerMock = new Mock<IComputer>();
        var hardwareMock = new Mock<IHardware>();
        var sensorMock = new Mock<ISensor>();
        hardwareMock.Setup(h => h.Name).Returns("TestHardware");
        hardwareMock.Setup(h => h.Sensors).Returns(new[] { sensorMock.Object });
        computerMock.Setup(c => c.Hardware).Returns(new[] { hardwareMock.Object });
        var cachedMetrics = new PrometheusWindowsHardwareExporter.CachedMetrics(computerMock.Object, TimeSpan.FromSeconds(10));
        var text = cachedMetrics.GetMetricsText();
        var lines = text.Split('\n');
        Assert.Equal(4, lines.Length);
        Assert.Equal("# TYPE prometheus_windows_hardware_exporter_up gauge", lines[0]);
        Assert.Equal("prometheus_windows_hardware_exporter_up 1", lines[1]);
        Assert.Equal("# TYPE prometheus_windows_hardware gauge", lines[2]);
        Assert.Equal("", lines[3]);
    }

    [Fact]
    public void TestCachedMetricsWithSimpleHardware()
    {
        var computerMock = new Mock<IComputer>();
        var hardwareMock = new Mock<IHardware>();
        var sensorMock = new Mock<ISensor>();
        hardwareMock.Setup(h => h.Name).Returns("TestHardware");
        hardwareMock.Setup(h => h.HardwareType).Returns(HardwareType.Cpu);
        sensorMock.Setup(s => s.Name).Returns("Test Sensor");
        sensorMock.Setup(s => s.Value).Returns(42.0f);
        sensorMock.Setup(s => s.SensorType).Returns(SensorType.Temperature);
        hardwareMock.Setup(h => h.Sensors).Returns(new[] { sensorMock.Object });
        computerMock.Setup(c => c.Hardware).Returns(new[] { hardwareMock.Object });
        var cachedMetrics = new PrometheusWindowsHardwareExporter.CachedMetrics(computerMock.Object, TimeSpan.FromSeconds(10));
        var text = cachedMetrics.GetMetricsText();
        var lines = text.Split('\n');
        Assert.Equal(5, lines.Length);
        Assert.Equal("# TYPE prometheus_windows_hardware_exporter_up gauge", lines[0]);
        Assert.Equal("prometheus_windows_hardware_exporter_up 1", lines[1]);
        Assert.Equal("# TYPE prometheus_windows_hardware gauge", lines[2]);
        Assert.Equal("prometheus_windows_hardware{hardware=\"TestHardware\",hardware_type=\"Cpu\",sensor=\"test_sensor\",sensor_type=\"Temperature\"} 42", lines[3]);
    }
}