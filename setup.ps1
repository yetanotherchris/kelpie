$ErrorActionPreference = "Stop"

$solutionFile      = "Kelpie.sln"
$configuration     = "Debug"
$platform          = "Mixed Platforms"
$msbuild           = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
$configTool        = ".\src\Kelpie.Web.IisConfig\bin\Debug\Kelpie.Web.IisConfig.exe"

# Install nuget to restore
Write-Host "Installing Nuget." -ForegroundColor Green
choco install nuget.commandline -y

if (!(Test-Path $msbuild))
{
	$msbuild = "C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
}

# Nuget restoring
Write-Host "Nuget restoring"
nuget restore $solutionFile

# Build the sln file
Write-Host "Building $solutionFile." -ForegroundColor Green
cd $PSScriptRoot
& $msbuild $solutionFile /p:Configuration=$configuration /p:Platform=$platform /target:Build /verbosity:quiet
if ($LastExitCode -ne 0)
{
	throw "Building solution failed."
}
else
{
	Write-Host "Building solution complete."-ForegroundColor Green
}

# Run IISConfig tool
Write-Host "Running the IIS setup tool." -ForegroundColor Green
& $configTool
if ($LastExitCode -ne 0)
{
	Write-Host "IISConfig setup failed."-ForegroundColor Red
	exit 1
}
else
{
	Write-Host "IIS Setup complete." -ForegroundColor Green
}

# Done
Write-host "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" -ForegroundColor DarkYellow
Write-Host "Setup complete." -ForegroundColor Green
Write-host "- Site: http://localhost:410/"
Write-Host "- RavenDB dashboard : http://localhost:411/"
Write-host "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" -ForegroundColor DarkYellow