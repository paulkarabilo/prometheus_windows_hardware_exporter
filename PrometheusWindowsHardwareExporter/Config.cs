using System.Text.Json.Serialization;

namespace PrometheusWindowsHardwareExporter
{
    public class Config
    {
        [JsonPropertyName("listen_address")]
        public string ListenAddress { get; set; } = "http://localhost:9182/";

        [JsonPropertyName("collect_interval")]
        public int CollectInterval { get; set; } = 15;
        [JsonPropertyName("metrics_path")]
        public string MetricsPath { get; set; } = "/metrics";

        [JsonPropertyName("collectors")]
        public string[] Collectors {get; set; } = new string[] {
            "cpu",
            "gpu",
            "memory",
            "disk",
            "motherboard"
        };
    }
}