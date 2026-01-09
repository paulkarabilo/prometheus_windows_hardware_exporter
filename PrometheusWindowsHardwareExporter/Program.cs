using System;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.ServiceProcess;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.PawnIo;
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
                IsPsuEnabled = enabledCollectors.Contains("psu"),
            };

            if (!PawnIo.IsInstalled || PawnIo.Version < new Version(2, 0, 0, 0))
            {
                Console.WriteLine("Warning: PawnIo driver is not installed or is outdated. Some hardware monitoring features may not work correctly.");
                Console.WriteLine("Go to https://pawnio.eu/ to get the latest version");
            }

            if (!IsElevated())
            {
                Console.WriteLine("Warning: Running in non-administrator role may result in some metrics being not available!");
            }

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

        private static bool IsElevated()
        {
            WindowsPrincipal p = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}