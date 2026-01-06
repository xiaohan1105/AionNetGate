@echo off
chcp 65001 >nul
title AionNetGate 单体应用

echo ========================================
echo   AionNetGate 单体应用
echo ========================================
echo.

REM 检查前端是否已构建
if not exist "src\AionNetGate.WebApi\wwwroot\index.html" (
    echo 前端未构建，正在构建...
    cd web
    call npm install
    call npm run build
    cd ..
)

echo 正在启动服务...
echo.
echo   Web 界面: http://localhost:5000
echo   API 文档: http://localhost:5000/swagger
echo.
echo 按 Ctrl+C 停止服务
echo.

set ASPNETCORE_URLS=http://0.0.0.0:5000
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src\AionNetGate.WebApi

pause
