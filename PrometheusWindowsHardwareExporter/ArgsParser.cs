using PrometheusWindowsHardwareExporter;

namespace PrometheusWindowsHardwareExporter
{
    public class ArgsParser
    {
        public static Config ParseArgs(string[] args)
        {
            Config config = new Config();
            foreach (string a in args)
            {
                if (a.StartsWith("--config="))
                {
                    string configPath = a.Substring("--config=".Length);
                    if (File.Exists(configPath))
                    {
                        string json = File.ReadAllText(configPath);
                        config = System.Text.Json.JsonSerializer.Deserialize<Config>(json, ConfigGenerationContext.Default.Config) ?? config;
                    }
                }

                if (a.StartsWith("--listen-address="))
                {
                    config.ListenAddress = a.Substring("--listen-address=".Length);
                }

                if (a.StartsWith("--collect-interval="))
                {
                    if (int.TryParse(a.Substring("--collect-interval=".Length), out int interval))
                    {
                        config.CollectInterval = interval;
                    }
                }

                if (a.StartsWith("--metrics-path="))
                {
                    config.MetricsPath = a.Substring("--metrics-path=".Length);
                }
                
                if (a.StartsWith("--collectors="))
                {
                    string collectors = a.Substring("--collectors=".Length);
                    config.Collectors = collectors.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                if (a.Equals("--service"))
                {
                    config.Service = true;
                }

                if (a.Equals("--help"))
                {
                    config.ShowHelp = true;
                }
            }
            return config;
        }
    }
}