# AionNetGate 单体应用启动脚本
# 用法: .\start.ps1

param(
    [int]$Port = 5000,
    [switch]$Release,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$Configuration = if ($Release) { "Release" } else { "Debug" }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AionNetGate 单体应用" -ForegroundColor Cyan
Write-Host "  端口: $Port" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 检查前端是否已构建
$WwwrootPath = "$ProjectRoot\src\AionNetGate.WebApi\wwwroot"
if (-not (Test-Path "$WwwrootPath\index.html")) {
    Write-Host "`n前端未构建，正在构建..." -ForegroundColor Yellow
    & "$ProjectRoot\build.ps1"
}

# 设置环境变量
$env:ASPNETCORE_URLS = "http://0.0.0.0:$Port"
$env:ASPNETCORE_ENVIRONMENT = if ($Release) { "Production" } else { "Development" }

Write-Host "`n正在启动服务..." -ForegroundColor Yellow
Write-Host "  API 地址: http://localhost:$Port/api" -ForegroundColor Gray
Write-Host "  Web 界面: http://localhost:$Port" -ForegroundColor Gray
Write-Host "  Swagger:  http://localhost:$Port/swagger" -ForegroundColor Gray
Write-Host "  健康检查: http://localhost:$Port/health" -ForegroundColor Gray
Write-Host "`n按 Ctrl+C 停止服务`n" -ForegroundColor Gray

# 启动
if ($NoBuild) {
    dotnet run --project "$ProjectRoot\src\AionNetGate.WebApi" -c $Configuration --no-build
} else {
    dotnet run --project "$ProjectRoot\src\AionNetGate.WebApi" -c $Configuration
}
