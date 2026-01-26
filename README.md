# Prometheus Windows Hardware Exporter

A Windows hardware monitoring exporter for Prometheus that exposes system metrics including CPU, GPU, memory, storage, network, and more.

## Overview

Prometheus Windows Hardware Exporter is a .NET application that monitors Windows hardware components and exposes their metrics in Prometheus format. It leverages LibreHardwareMonitor to gather detailed hardware information and presents it via an HTTP endpoint for Prometheus scraping.

## Features

- **Multi-component monitoring**: CPU, GPU, Memory, Motherboard, Storage, PSU, Battery, Network
- **Configurable collectors**: Enable/disable specific hardware monitors
- **Console and Service modes**: Run as a console application or Windows service
- **Configuration file support**: JSON-based configuration
- **Customizable metrics endpoint**: Configure listening address and metrics path
- **Hardware-aware**: Detects missing or outdated drivers (e.g., PawnIo driver)

## Metrics

The hardware exporter provides a single gauge metric called `prometheus_windows_hardware` with the following labels: `hardware` (e.g. `Intel Core i7-7700`, `hardware_type` (e.g. `Cpu, NvidiaGpu`, `Storage` etc), `sensor` (e.g. `cpu_core_max`, `data_read`, etc) and `sensor_type` (e.g. `Temperature`, `Clock`, `Load`, etc). The values of the labels are provided directly by LibreHaedwareMonitor library

## Installation

### From Github Releases

1. Download the latest release from the [Github Releases page](https://github.com/paulkarabilo/prometheus_windows_hardware_exporter/releases)
2. Extract the archive to your desired location
3. Run the installer to install as a Windows service, or run manually

## Usage

### Console Mode

```bash
PrometheusWindowsHardwareExporter.exe [options]
```

### Service Mode

Install and run as a Windows service:

```bash
PrometheusWindowsHardwareExporter.exe --service
```

## Command-Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `--help` | Display help information | - |
| `--service` | Run as a Windows service | Console mode |
| `--config=<path>` | Path to JSON configuration file | Built-in defaults |
| `--listen-address=<url>` | HTTP endpoint to listen on | `http://localhost:9182/` |
| `--metrics-path=<path>` | Metrics endpoint path | `/metrics` |
| `--collect-interval=<seconds>` | Hardware metrics collection interval | `15` |
| `--collectors=<list>` | Comma-separated list of collectors to enable | All enabled |

### Collectors

Available collectors:
- `cpu` - CPU usage and temperature
- `gpu` - GPU metrics
- `memory` - RAM usage and details
- `motherboard` - Motherboard sensors
- `storage` - Disk usage and health
- `network` - Network interface statistics
- `battery` - Battery status (for laptops)
- `psu` - Power supply metrics

**Example**:
```bash
PrometheusWindowsHardwareExporter.exe --collectors=cpu,memory,storage
```

## Configuration File

Create a JSON configuration file for persistent settings:

```json
{
  "listenAddress": "http://localhost:9182/",
  "metricsPath": "/metrics",
  "collectInterval": 15,
  "collectors": ["cpu", "gpu", "memory", "motherboard", "storage", "network", "battery", "psu"],
  "maxConcurrent": 10,
  "requestTimeout": "00:00:10"
}
```

Load the configuration file:

```bash
PrometheusWindowsHardwareExporter.exe --config=C:\path\to\config.json
```

### Configuration Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `listenAddress` | string | HTTP endpoint URL | `http://localhost:9182/` |
| `metricsPath` | string | Metrics endpoint path | `/metrics` |
| `collectInterval` | int | Collection interval in seconds | `15` |
| `collectors` | array | List of enabled collectors | All collectors |
| `maxConcurrent` | int | Maximum concurrent operations | `10` |
| `requestTimeout` | string | Request timeout (ISO 8601 format) | `00:00:10` |

## Prometheus Configuration

Add the following to your Prometheus `prometheus.yml`:

```yaml
scrape_configs:
  - job_name: 'windows-hardware'
    static_configs:
      - targets: ['your-windows-machine-ip-or-hostname]:9182']
    scrape_interval: 30s
    scrape_timeout: 10s
```

## Requirements

- **OS**: Windows 7 or later (Windows 10+ recommended)
- **.NET Runtime**: .NET 10.0 or compatible
- **Administrator privileges**: Recommended for full hardware access
- **PawnIo Driver** (optional): Version 2.0.0 or later for enhanced hardware monitoring
  - Download from: https://pawnio.eu/

## Warnings

- Running without administrator privileges may result in some metrics being unavailable
- If PawnIo driver is not installed or outdated, some hardware monitoring features may be limited
- The first metrics collection after startup may take longer as the exporter initializes hardware monitors

## License

See LICENSE and THIRD-PARTY-NOTICES.txt for licensing information.

## Dependencies

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - Hardware monitoring library
- Prometheus client libraries - Metrics exposition

## Building from Source

```bash
dotnet build PrometheusWindowsHardwareExporter.sln
```

## Support & Issues

For bug reports, feature requests, or other issues, please visit the project's Github repository.

---

**Note**: This application requires Windows and administrator privileges for optimal operation. Some hardware metrics may not be available on virtual machines or systems with restricted driver access.
