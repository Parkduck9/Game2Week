<#
.SYNOPSIS
  Unity Windows 빌드(Builds/Windows)를 MSIX 패키지로 포장한다.

.EXAMPLE
  # 집에서 설치·삭제 테스트용 (테스트 인증서를 만들어 서명)
  powershell -ExecutionPolicy Bypass -File Tools/package_msix.ps1 -Version 0.1.0.0 -CreateTestCert

.EXAMPLE
  # 스토어 제출용 (서명하지 않음 — 스토어가 서명). 파트너 센터 "제품 ID" 값을 그대로 넣는다
  powershell -ExecutionPolicy Bypass -File Tools/package_msix.ps1 -Version 1.0.0.0 -NoSign `
    -Name "12345Publisher.GameName" -Publisher "CN=XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX" -PublisherDisplayName "게시자 이름"

.NOTES
  필요: Windows SDK (makeappx.exe, signtool.exe). 결과: Builds/Msix/<Name>_<Version>_x64.msix
  테스트 인증서를 "신뢰할 수 있는 사람"에 등록하는 단계(관리자 권한)는 안내만 출력하고 직접 하지 않는다.
#>
param(
    [string]$BuildDir = "Builds/Windows",
    [string]$OutDir = "Builds/Msix",
    [string]$Version = "",                              # 생략하면 빌드 제품 설정 사용
    [string]$Name = "Game2Week.Dev",                      # 스토어: 파트너 센터 Package/Identity/Name
    [string]$Publisher = "CN=Game2Week Dev",              # 스토어: 파트너 센터 Package/Identity/Publisher
    [string]$PublisherDisplayName = "Game2Week",
    [string]$DisplayName = "타이틀 (가제)",
    [string]$Description = "3D 턴제 전투 게임",
    [switch]$NoSign,
    [switch]$CreateTestCert,
    [string]$CertPath = "Builds/Msix/TestCert.pfx",
    [string]$CertPassword = "game2week-test"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $root

$productPath = Join-Path $root "Builds/product_info.json"
if (-not (Test-Path -LiteralPath $productPath)) { throw "제품 설정이 없습니다. 먼저 새 Windows 빌드를 실행하세요." }
$product = Get-Content -LiteralPath $productPath -Raw -Encoding UTF8 | ConvertFrom-Json
$productDefaults = @{ Name=$product.packageIdentity; Publisher=$product.packagePublisher; PublisherDisplayName=$product.publisherDisplayName; DisplayName=$product.displayName; Description=$product.description }
foreach ($setting in $productDefaults.Keys) {
    if (-not $PSBoundParameters.ContainsKey($setting)) { Set-Variable -Name $setting -Value $productDefaults[$setting] }
}
if (-not $Version) { $productVersion=[version]$product.version; $Version="{0}.{1}.{2}.0" -f $productVersion.Major,$productVersion.Minor,$productVersion.Build }
if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') { throw "Version은 a.b.c.d 형식이어야 해요: $Version" }

# ---------- 도구 찾기 ----------
function Find-SdkTool([string]$exe) {
    $kits = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
    $found = Get-ChildItem $kits -Recurse -Filter $exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\' } | Sort-Object FullName -Descending | Select-Object -First 1
    if (-not $found) { throw "$exe 를 찾을 수 없어요. Windows SDK를 설치하세요 (Visual Studio Installer ▸ 개별 구성 요소 ▸ Windows SDK)." }
    return $found.FullName
}
$makeappx = Find-SdkTool "makeappx.exe"

# ---------- 빌드 확인 ----------
$exe = Get-ChildItem $BuildDir -Filter *.exe -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne "UnityCrashHandler64.exe" } | Select-Object -First 1
if (-not $exe) { throw "$BuildDir 에 게임 exe가 없어요. 먼저 Unity 빌드(Tools/build_windows.ps1)를 하세요." }

# ---------- 스테이징 ----------
$staging = Join-Path $OutDir "staging"
$resolvedStaging = [IO.Path]::GetFullPath((Join-Path $root $staging))
if (-not $resolvedStaging.StartsWith($root + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw "스테이징 경로는 프로젝트 폴더 안이어야 합니다." }
if (Test-Path -LiteralPath $resolvedStaging) { Remove-Item -LiteralPath $resolvedStaging -Recurse -Force }
New-Item -ItemType Directory -Force $staging | Out-Null
Get-ChildItem $BuildDir | Where-Object { $_.Name -notlike "*_DoNotShip" -and $_.Name -notlike "*_BackUpThisFolder*" } |
    Copy-Item -Destination $staging -Recurse -Force

# 로고: Tools/msix/Images에 있으면 그것을, 없으면 임시 로고를 만든다
$images = Join-Path $staging "Images"
New-Item -ItemType Directory -Force $images | Out-Null
$customImages = Join-Path $PSScriptRoot "msix\Images"
$logoSizes = @{ "StoreLogo.png" = @(50, 50); "Square44x44Logo.png" = @(44, 44); "Square150x150Logo.png" = @(150, 150); "Wide310x150Logo.png" = @(310, 150) }
Add-Type -AssemblyName System.Drawing
foreach ($file in $logoSizes.Keys) {
    $custom = Join-Path $customImages $file
    if (Test-Path $custom) { Copy-Item $custom (Join-Path $images $file); continue }
    $w, $h = $logoSizes[$file]
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = "AntiAlias"
    $g.Clear([System.Drawing.Color]::FromArgb(43, 47, 61))
    $s = [Math]::Min($w, $h) * 0.32
    $cx = $w / 2; $cy = $h / 2
    $pts = [System.Drawing.PointF[]]@((New-Object System.Drawing.PointF $cx, ($cy - $s)), (New-Object System.Drawing.PointF ($cx + $s * 0.7), $cy),
        (New-Object System.Drawing.PointF $cx, ($cy + $s)), (New-Object System.Drawing.PointF ($cx - $s * 0.7), $cy))
    $g.FillPolygon((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(246, 212, 102))), $pts)
    $g.Dispose()
    $bmp.Save((Join-Path $images $file), [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

# 매니페스트
$manifest = Get-Content (Join-Path $PSScriptRoot "msix\AppxManifest.template.xml") -Raw -Encoding UTF8
$values = @{ Name = $Name; Publisher = $Publisher; Version = $Version; DisplayName = $DisplayName; PublisherDisplayName = $PublisherDisplayName; Description = $Description; Executable = $exe.Name }
foreach ($k in $values.Keys) { $manifest = $manifest.Replace("{{$k}}", [System.Security.SecurityElement]::Escape($values[$k])) }
[IO.File]::WriteAllText((Join-Path $staging "AppxManifest.xml"), $manifest, (New-Object Text.UTF8Encoding $false))

# ---------- 포장 ----------
$msix = Join-Path $OutDir ("{0}_{1}_x64.msix" -f $Name, $Version)
& $makeappx pack /d $staging /p $msix /o /h SHA256 | Out-Host
if ($LASTEXITCODE -ne 0) { throw "makeappx 실패 ($LASTEXITCODE)" }

# ---------- 서명 ----------
if ($NoSign) {
    Write-Host "`n[완료] 서명하지 않은 패키지 (스토어 제출용): $msix"
    return
}

if ($CreateTestCert -and -not (Test-Path $CertPath)) {
    # 현재 사용자 인증서 저장소에 테스트 인증서를 만들고 pfx로 내보낸다 (Publisher와 Subject가 같아야 함)
    $cert = New-SelfSignedCertificate -Type Custom -Subject $Publisher -KeyUsage DigitalSignature -FriendlyName "Game2Week MSIX Test" `
        -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    $secure = ConvertTo-SecureString $CertPassword -AsPlainText -Force
    Export-PfxCertificate -Cert $cert -FilePath $CertPath -Password $secure | Out-Null
    Export-Certificate -Cert $cert -FilePath ([IO.Path]::ChangeExtension($CertPath, ".cer")) | Out-Null
    Write-Host "테스트 인증서 생성: $CertPath"
}
if (-not (Test-Path $CertPath)) { throw "인증서가 없어요: $CertPath (-CreateTestCert 를 붙이거나 -NoSign)" }

$signtool = Find-SdkTool "signtool.exe"
& $signtool sign /fd SHA256 /a /f $CertPath /p $CertPassword $msix | Out-Host
if ($LASTEXITCODE -ne 0) { throw "signtool 실패 ($LASTEXITCODE)" }

$cer = [IO.Path]::ChangeExtension($CertPath, ".cer")
Write-Host @"

[완료] 서명된 테스트 패키지: $msix

설치 전에 한 번만 (관리자 PowerShell):
  Import-Certificate -FilePath "$cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople
그다음 .msix를 더블클릭하거나:
  Add-AppxPackage "$msix"
"@
