using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrometheusWindowsHardwareExporter
{
    internal sealed class CachedMetrics
    {
        private readonly object _lock = new();
        private readonly Computer _computer;
        private readonly TimeSpan _minInterval;
        private readonly UpdateVisitor _visitor = new();

        private long _lastUpdateTicks;
        private string _cached = "# TYPE prometheus_windows_hardware_exporter_up gauge\nprometheus_windows_hardware_exporter_up 1\n";
        public CachedMetrics(Computer computer, TimeSpan minInterval)
        {
            _computer = computer;
            _minInterval = minInterval;
        }

        public string GetMetricsText()
        {
            long now = Stopwatch.GetTimestamp();
            lock (_lock)
            {
                if (_lastUpdateTicks != 0)
                {
                    double elapsedSeconds = (now - _lastUpdateTicks) / (double)Stopwatch.Frequency;
                    if (elapsedSeconds < _minInterval.TotalSeconds)
                    {
                        return _cached;
                    }
                }
                _lastUpdateTicks = now;
                _cached = Build();
                return _cached;
            }
        }

        private string Build()
        {
            try
            {
                _computer.Accept(_visitor);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# TYPE prometheus_windows_hardware_exporter_up gauge\nprometheus_windows_hardware_exporter_up 1");
                sb.AppendLine("# TYPE prometheus_windows_hardware_exporter_temperature gauge");

                foreach (IHardware hw in _computer.Hardware)
                    EmitTemps(sb, hw);
                return sb.ToString().Replace("\r\n", "\n");
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# TYPE prometheus_windows_hardware_exporter_up gauge");
                sb.AppendLine("prometheus_windows_hardware_exporter_up 0");
                sb.AppendLine("# TYPE prometheus_windows_hardware_exporter_error_info gauge");
                sb.AppendLine($"prometheus_windows_hardware_exporter_error_info{{error=\"{ex.Message.Replace("\"", "\\\"")}\"}} 1");
                return sb.ToString().Replace("\r\n", "\n");
            }
        }

        private string EmitTemps(StringBuilder sb, IHardware hw)
        {
            foreach (ISensor sensor in hw.Sensors)
            {
                Console.WriteLine($"Hardware: {hw.Name} Sensor: {sensor.Name} Type: {sensor.SensorType} Value: {sensor.Value}");
                if (sensor.Value != null && sensor.Value.HasValue)
                {
                    string name = sensor.Name.Replace(' ', '_').ToLowerInvariant();
                    sb.Append("prometheus_windows_hardware{")
                        .Append("hardware=\"")
                        .Append(SanitizeLabelValue(hw.Name))
                        .Append("\",hardware_type=\"")
                        .Append(SanitizeLabelValue(hw.HardwareType.ToString()))
                        .Append("\",sensor=\"")
                        .Append(SanitizeLabelValue(name))
                        .Append("\",sensor_type=\"")
                        .Append(SanitizeLabelValue(sensor.SensorType.ToString()))
                        .Append("\"} ")
                        .Append(sensor.Value.Value.ToString())
                        .Append("\n");
                }
                foreach (IHardware subHw in hw.SubHardware)
                {
                    EmitTemps(sb, subHw);
                }
            }
            return sb.ToString();
        }

        private static string SanitizeLabelValue(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        private sealed class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware)
                {
                    subHardware.Accept(this);
                }
            }
            public void VisitSensor(ISensor sensor)
            {
                // No action needed for sensors in this visitor
            }
            public void VisitParameter(IParameter parameter)
            {
                // No action needed for parameters in this visitor
            }
        }
    }
}
