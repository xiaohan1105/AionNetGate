@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: AionNetGate Windows 部署脚本
:: 用法: deploy.bat [命令]

cd /d "%~dp0.."

set "CMD=%~1"
if "%CMD%"=="" set "CMD=start"

if "%CMD%"=="start" goto :start
if "%CMD%"=="stop" goto :stop
if "%CMD%"=="restart" goto :restart
if "%CMD%"=="status" goto :status
if "%CMD%"=="logs" goto :logs
if "%CMD%"=="monitor" goto :monitor
if "%CMD%"=="build" goto :build
goto :help

:start
echo [INFO] 启动 AionNetGate 服务...
call :check_env
docker-compose up -d gateway mysql
echo.
echo [INFO] 服务已启动
echo [INFO] 网关端口: 10001
echo [INFO] 管理API: http://localhost:11001
echo [INFO] 健康检查: http://localhost:11001/health
goto :eof

:stop
echo [INFO] 停止 AionNetGate 服务...
docker-compose down
echo [INFO] 服务已停止
goto :eof

:restart
echo [INFO] 重启 AionNetGate 服务...
docker-compose restart
echo [INFO] 服务已重启
goto :eof

:status
echo [INFO] 服务状态:
docker-compose ps
echo.
echo [INFO] 健康检查:
curl -s http://localhost:11001/health 2>nul || echo 服务未运行或无法访问
goto :eof

:logs
set "SVC=%~2"
if "%SVC%"=="" set "SVC=gateway"
docker-compose logs -f %SVC%
goto :eof

:monitor
echo [INFO] 启动完整服务 (包含监控)...
call :check_env
docker-compose --profile monitoring up -d
echo.
echo [INFO] 服务已启动
echo [INFO] 网关端口: 10001
echo [INFO] 管理API: http://localhost:11001
echo [INFO] Prometheus: http://localhost:9090
echo [INFO] Grafana: http://localhost:3000 (admin/admin123)
goto :eof

:build
echo [INFO] 构建 Docker 镜像...
docker-compose build --no-cache
echo [INFO] 构建完成
goto :eof

:check_env
if not exist ".env" (
    echo [WARN] .env 文件不存在，从示例文件创建...
    if exist ".env.example" (
        copy .env.example .env >nul
        echo [WARN] 请编辑 .env 文件设置正确的密钥和密码
        exit /b 1
    ) else (
        echo [ERROR] 未找到 .env.example 文件
        exit /b 1
    )
)
goto :eof

:help
echo AionNetGate 部署脚本
echo.
echo 用法: deploy.bat [命令]
echo.
echo 命令:
echo   start     启动基础服务 (网关 + MySQL)
echo   stop      停止所有服务
echo   restart   重启所有服务
echo   status    查看服务状态
echo   logs      查看日志 (可选: logs gateway^|mysql^|prometheus)
echo   monitor   启动完整服务 (包含 Prometheus + Grafana)
echo   build     重新构建 Docker 镜像
goto :eof
