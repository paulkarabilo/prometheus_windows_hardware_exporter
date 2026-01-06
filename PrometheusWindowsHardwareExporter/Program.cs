using System;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using LibreHardwareMonitor.Hardware;
using PrometheusWindowsHardwareExporter;

namespace PrometheusWindowsHardwareExporter
{

    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            bool runAsService = args.Any(a => a.Equals("--service", StringComparison.OrdinalIgnoreCase));
            Config config = ArgsParser.ParseArgs(args);

            HashSet<string> enabledCollectors = new HashSet<string>(config.Collectors, StringComparer.OrdinalIgnoreCase);

            Computer computer = new Computer
            {
                IsCpuEnabled = enabledCollectors.Contains("cpu"),
                IsGpuEnabled = enabledCollectors.Contains("gpu"),
                IsMemoryEnabled = enabledCollectors.Contains("memory"),
                IsMotherboardEnabled = enabledCollectors.Contains("motherboard"),
                IsStorageEnabled = enabledCollectors.Contains("storage"),
                IsNetworkEnabled = enabledCollectors.Contains("network"),
                IsBatteryEnabled = enabledCollectors.Contains("battery"),
            };

            computer.Open();

            CachedMetrics metrics = new CachedMetrics(computer, TimeSpan.FromSeconds(config.CollectInterval));

            if (config.Service == true && OperatingSystem.IsWindows())
            { 
                ServiceBase.Run(new Service(config, metrics));
                return;
            }

            await RunConsoleAsync(config, metrics);
        }

        private static async Task RunConsoleAsync(Config config, CachedMetrics metrics)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            ExporterHost host = new ExporterHost(config, metrics);
            await host.RunAsync(cts.Token);
        }
    }
}