# AionNetGate 单体应用一键构建脚本
# 用法: .\build.ps1

param(
    [switch]$Release,
    [switch]$SkipFrontend
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$Configuration = if ($Release) { "Release" } else { "Debug" }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AionNetGate 单体应用构建" -ForegroundColor Cyan
Write-Host "  配置: $Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. 构建前端
if (-not $SkipFrontend) {
    Write-Host "`n[1/3] 构建前端..." -ForegroundColor Yellow
    Push-Location "$ProjectRoot\web"

    if (-not (Test-Path "node_modules")) {
        Write-Host "  安装npm依赖..." -ForegroundColor Gray
        npm install
        if ($LASTEXITCODE -ne 0) { throw "npm install 失败" }
    }

    Write-Host "  构建Vue应用..." -ForegroundColor Gray
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm build 失败" }

    Pop-Location
    Write-Host "  前端构建完成!" -ForegroundColor Green
} else {
    Write-Host "`n[1/3] 跳过前端构建" -ForegroundColor Gray
}

# 2. 恢复后端依赖
Write-Host "`n[2/3] 恢复后端依赖..." -ForegroundColor Yellow
dotnet restore "$ProjectRoot\src\AionNetGate.WebApi\AionNetGate.WebApi.csproj"
if ($LASTEXITCODE -ne 0) { throw "dotnet restore 失败" }
Write-Host "  依赖恢复完成!" -ForegroundColor Green

# 3. 构建后端
Write-Host "`n[3/3] 构建后端..." -ForegroundColor Yellow
dotnet build "$ProjectRoot\src\AionNetGate.WebApi\AionNetGate.WebApi.csproj" -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build 失败" }
Write-Host "  后端构建完成!" -ForegroundColor Green

# 输出结果
$OutputPath = "$ProjectRoot\src\AionNetGate.WebApi\bin\$Configuration\net9.0"
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  构建成功!" -ForegroundColor Green
Write-Host "  输出目录: $OutputPath" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n启动命令:" -ForegroundColor Yellow
Write-Host "  dotnet run --project src\AionNetGate.WebApi -c $Configuration" -ForegroundColor White
Write-Host "`n或直接运行:" -ForegroundColor Yellow
Write-Host "  .\start.ps1" -ForegroundColor White
