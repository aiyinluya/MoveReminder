param(
    [string]$Project = ".\src\MoveReminder\MoveReminder.csproj",
    [string]$OutputDir = ".\artifacts\MoveReminder-publish",
    [string]$Runtime = "win-x64",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Push-Location $repoRoot

try {
    $dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
    if (-not $dotnet) {
        $dotnet = Join-Path $env:ProgramFiles "dotnet\dotnet.exe"
    }
    if (-not (Test-Path $dotnet)) {
        throw "dotnet SDK not found. Install .NET 8 SDK or add dotnet to PATH."
    }

    & $dotnet publish $Project `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
        /p:PublishSingleFile=true `
        /p:IncludeNativeLibrariesForSelfExtract=true `
        -o $OutputDir

    $exe = Join-Path $OutputDir "MoveReminder.exe"
    if (Test-Path $exe) {
        Write-Host "Published: $exe"
    }
}
finally {
    Pop-Location
}
