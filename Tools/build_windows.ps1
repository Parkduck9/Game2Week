<#
.SYNOPSIS
  Unity를 배치모드로 실행해 Windows 64비트 빌드를 만든다 → Builds/Windows
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Tools/build_windows.ps1 -Version 0.1.0
.NOTES
  Unity 에디터로 프로젝트를 열어 둔 상태에서는 실행할 수 없어요 (프로젝트 잠금). 에디터를 닫고 실행하세요.
#>
param(
    [string]$Version = "",
    [string]$UnityVersion = "6000.3.25f1"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$unity = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity $UnityVersion 을 찾을 수 없어요: $unity (Unity Hub에서 설치)" }

$log = Join-Path $root "Logs\build_windows.log"
New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null
$args = @("-batchmode", "-quit", "-projectPath", "`"$root`"", "-executeMethod", "Game2Week.EditorTools.Build.BuildScript.BuildWindows", "-logFile", "`"$log`"")
if ($Version) { $args += @("-buildVersion", $Version) }

Write-Host "Unity 빌드 중... (로그: $log)"
$p = Start-Process -FilePath $unity -ArgumentList $args -Wait -PassThru
Select-String -Path $log -Pattern '\[Build\]|error CS' | ForEach-Object { $_.Line }
if ($p.ExitCode -ne 0) { throw "빌드 실패 (exit $($p.ExitCode)) — 로그 확인: $log" }
Write-Host "완료: $(Join-Path $root 'Builds\Windows')"
