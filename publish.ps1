# AionNetGate 单体应用发布脚本
# 生成可直接运行的单文件exe
# 用法: .\publish.ps1

param(
    [ValidateSet("win-x64", "win-x86", "linux-x64", "osx-x64")]
    [string]$Runtime = "win-x64",
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$OutputDir = "$ProjectRoot\publish\$Runtime"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AionNetGate 发布" -ForegroundColor Cyan
Write-Host "  目标平台: $Runtime" -ForegroundColor Cyan
Write-Host "  独立部署: $SelfContained" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. 构建前端
Write-Host "`n[1/2] 构建前端..." -ForegroundColor Yellow
Push-Location "$ProjectRoot\web"

if (-not (Test-Path "node_modules")) {
    npm install
}
npm run build
if ($LASTEXITCODE -ne 0) { throw "前端构建失败" }

Pop-Location
Write-Host "  前端构建完成!" -ForegroundColor Green

# 2. 发布后端
Write-Host "`n[2/2] 发布后端..." -ForegroundColor Yellow

$PublishArgs = @(
    "publish",
    "$ProjectRoot\src\AionNetGate.WebApi\AionNetGate.WebApi.csproj",
    "-c", "Release",
    "-r", $Runtime,
    "-o", $OutputDir,
    "/p:PublishSingleFile=true",
    "/p:IncludeNativeLibrariesForSelfExtract=true"
)

if ($SelfContained) {
    $PublishArgs += "--self-contained", "true"
    $PublishArgs += "/p:EnableCompressionInSingleFile=true"
} else {
    $PublishArgs += "--self-contained", "false"
}

& dotnet @PublishArgs
if ($LASTEXITCODE -ne 0) { throw "发布失败" }

Write-Host "  后端发布完成!" -ForegroundColor Green

# 复制配置文件
Copy-Item "$ProjectRoot\src\AionNetGate.WebApi\appsettings.json" "$OutputDir\" -Force
Copy-Item "$ProjectRoot\src\AionNetGate.WebApi\appsettings.Development.json" "$OutputDir\" -ErrorAction SilentlyContinue

# 输出结果
$ExeName = if ($Runtime -like "win*") { "AionNetGate.WebApi.exe" } else { "AionNetGate.WebApi" }
$ExePath = "$OutputDir\$ExeName"
$Size = [math]::Round((Get-Item $ExePath).Length / 1MB, 2)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  发布成功!" -ForegroundColor Green
Write-Host "  输出目录: $OutputDir" -ForegroundColor Gray
Write-Host "  可执行文件: $ExeName ($Size MB)" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n运行方式:" -ForegroundColor Yellow
Write-Host "  cd $OutputDir" -ForegroundColor White
Write-Host "  .\$ExeName" -ForegroundColor White
