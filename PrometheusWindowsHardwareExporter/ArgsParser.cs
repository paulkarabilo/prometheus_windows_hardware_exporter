using PrometheusWindowsHardwareExporter;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
                        var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();
                        var p = deserializer.Deserialize<Config>(File.ReadAllText(configPath));
                        if (p != null) {
                            config = p;
                        }
                    }
                }

                if (a.StartsWith("--web.listen-address="))
                {
                    config.Web.ListenAddress = a.Substring("--web.listen-address=".Length);
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
                    config.Web.MetricsPath = a.Substring("--metrics-path=".Length);
                }
                
                if (a.StartsWith("--collectors="))
                {
                    string collectors = a.Substring("--collectors=".Length);
                    config.Collectors.Enabled = collectors.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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