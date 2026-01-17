param(
    [string]$Configuration = "Release",
    [string]$publishDir = "",
    [string]$artifactsDir = "",
    [string]$version = "0.0.0.0",
    [bool]$pack = $false
)

dotnet restore ./PrometheusWindowsHardwareExporter/PrometheusWindowsHardwareExporter.csproj
dotnet build ./PrometheusWindowsHardwareExporter/PrometheusWindowsHardwareExporter.csproj -c $Configuration -r win-x64 --no-restore

New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
dotnet publish ./PrometheusWindowsHardwareExporter/PrometheusWindowsHardwareExporter.csproj --no-restore --no-build -c $Configuration -r win-x64 --self-contained true -o $publishDir

New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null

dotnet tool restore
dotnet tool run wix build ./Installer/Package.wxs -o (Join-Path $artifactsDir "PrometheusWindowsHardwareExporter.msi") -define PublishDir=$publishDir -define Version="$version"

if ($pack) {
    New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null
    $zipPath = Join-Path $artifactsDir "PrometheusWindowsHardwareExporter-win-x64.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath }
    Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath
}