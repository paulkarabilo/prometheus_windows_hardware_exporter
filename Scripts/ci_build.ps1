param(
    [string]$Configuration = "Release",
    [string]$publishDir = "",
    [string]$artifactsDir = "",
    [string]$version = "0.0.0.0",
    [bool]$pack = $false,
    [bool]$test = $true
)

dotnet restore ./PrometheusWindowsHardwareExporter/PrometheusWindowsHardwareExporter.csproj
dotnet build ./PrometheusWindowsHardwareExporter/PrometheusWindowsHardwareExporter.csproj -c $Configuration -r win-x64 --no-restore

if ($test) {
    $testResultsDir = Join-Path $artifactsDir "TestResults"
    New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null
    dotnet restore ./PrometheusWindowsHardwareExporter.Tests/PrometheusWindowsHardwareExporter.Tests.csproj
    dotnet build ./PrometheusWindowsHardwareExporter.Tests/PrometheusWindowsHardwareExporter.Tests.csproj -c Debug --no-restore
    dotnet test ./PrometheusWindowsHardwareExporter.Tests/PrometheusWindowsHardwareExporter.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=TestResults.trx" --results-directory $testResultsDir --collect:"XPlat Code Coverage;Format=json,lcov,cobertura" 
    dotnet tool restore./PrometheusWindowsHardwareExporter.Tests/PrometheusWindowsHardwareExporter.Tests.csproj
    dotnet tool run reportgenerator -reports:([IO.Path]::Combine($testResultsDir, "**", "coverage.cobertura.xml")) -targetdir:"coveragereport" -reporttypes:"Html;Cobertura;MarkdownSummaryGithub"
}

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