@echo off
echo ===============================================
echo 网关连接测试脚本
echo ===============================================
echo.

echo 1. 检查网关配置...
findstr "server_port" AionNetGate\Configs\Config.cs
echo.

echo 2. 检查登录器配置...
findstr "ServerPort" AionLanucher\Configs\Config.cs
echo.

echo 3. 端口匹配检查...
set /p gateway_port="请输入网关当前端口设置: "
set /p launcher_port="请输入登录器当前端口设置: "

if "%gateway_port%"=="%launcher_port%" (
    echo [✓] 端口匹配正确！
) else (
    echo [✗] 端口不匹配！需要修复
    echo 网关端口: %gateway_port%
    echo 登录器端口: %launcher_port%
)

echo.
echo 4. 测试建议：
echo - 建议使用端口 8000 作为标准端口
echo - 确保防火墙允许该端口
echo - 本地测试使用 IP: 127.0.0.1
echo.

echo 5. 启动顺序：
echo 1) 先启动网关程序
echo 2) 在网关中启动服务
echo 3) 启动生成的登录器
echo 4) 检查网关客户端列表是否显示连接
echo.

pause