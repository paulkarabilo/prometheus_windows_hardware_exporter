using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace PrometheusWindowsHardwareExporter
{
    internal class Service : ServiceBase
    {
        private readonly Config _config;
        private readonly CachedMetrics _metrics;
        private ExporterHost?  _host;
        private CancellationTokenSource? _cts;
        private Task? _task;

        public Service(Config config, CachedMetrics metrics)
        {
            _config = config;
            _metrics = metrics;
#if WINDOWS
            ServiceName = "PrometheusWindowsHardwareExporter";
            CanStop = true;
            AutoLog = true;
#endif
        }

        protected override void OnStart(string[] args)
        {
            _cts = new CancellationTokenSource();
            _host = new ExporterHost(_config, _metrics);
            _task = _host.RunAsync(_cts.Token);
        }

        protected override void OnStop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _task?.Wait(TimeSpan.FromSeconds(10)); } catch { }
            try { _host?.Dispose(); } catch { }
        }
    }
}
