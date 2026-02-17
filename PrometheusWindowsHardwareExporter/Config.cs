using YamlDotNet.Serialization;
namespace PrometheusWindowsHardwareExporter
{
    public class WebConfig
    {
        [YamlMember(Alias = "listen-address", ApplyNamingConventions = false )]
        public string ListenAddress { get; set; } = "http://localhost:9182/";

        [YamlMember(Alias = "metrics-path", ApplyNamingConventions = false)]
        public string MetricsPath { get; set; } = "/metrics";
    }

    public class CollectorsConfig
    {
        [YamlMember(Alias = "enabled", ApplyNamingConventions = false)]
        public string[] Enabled { get; set; } = new string[] {
            "cpu",
            "gpu",
            "memory",
            "motherboard",
            "storage",
            "psu",
            "battery",
            "network"
        };
    }

    public class LogConfig
    {
        [YamlMember(Alias = "level", ApplyNamingConventions = false)]
        public string Level { get; set; } = "info";
    }

    public class Config
    {
        [YamlMember(Alias = "web", ApplyNamingConventions = false)]
        public WebConfig Web { get; set; } = new WebConfig();

        [YamlMember(Alias = "collect-interval", ApplyNamingConventions = false)]
        public int CollectInterval { get; set; } = 15;

        [YamlMember(Alias = "collectors", ApplyNamingConventions = false)]
        public CollectorsConfig Collectors { get; set; } = new CollectorsConfig();

        [YamlMember(Alias = "log", ApplyNamingConventions = false)]
        public LogConfig Log { get; set; } = new LogConfig();

        [YamlMember(Alias = "service", ApplyNamingConventions = false)]
        public bool Service { get; set; } = false;

        public bool ShowHelp { get; set; } = false;

        public int MaxConcurrent { get; internal set; } = 10;

        public TimeSpan? RequestTimeout { get; internal set; } = TimeSpan.FromSeconds(10);
    }
}