using System.Text.Json;
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
            "motherboard",
            "storage",
            "psu",
            "battery",
            "network"
        };

        [JsonPropertyName("service")]
        public bool Service { get; set; } = false;

        public bool ShowHelp { get; set; } = false;

        [JsonPropertyName("max_concurrent")]
        public int MaxConcurrent { get; internal set; } = 10;

        [JsonPropertyName("request_timeout")]
        public TimeSpan? RequestTimeout { get; internal set; } = TimeSpan.FromSeconds(10);
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    [JsonSerializable(typeof(JsonElement))]
    internal partial class ConfigGenerationContext : JsonSerializerContext
    {
    }
}