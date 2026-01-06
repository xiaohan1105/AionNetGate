# AionNetGate 网关服务 Docker 镜像
# 多阶段构建，优化镜像大小

# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 复制项目文件并还原依赖
COPY ["src/AionNetGate.Core/AionNetGate.Core.csproj", "src/AionNetGate.Core/"]
COPY ["src/AionNetGate.Infrastructure/AionNetGate.Infrastructure.csproj", "src/AionNetGate.Infrastructure/"]
COPY ["src/AionNetGate.Application/AionNetGate.Application.csproj", "src/AionNetGate.Application/"]
COPY ["src/AionNetGate.Network/AionNetGate.Network.csproj", "src/AionNetGate.Network/"]
COPY ["src/AionNetGate.Host/AionNetGate.Host.csproj", "src/AionNetGate.Host/"]
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "src/AionNetGate.Host/AionNetGate.Host.csproj"

# 复制源代码并构建
COPY ["src/", "src/"]
COPY ["Directory.Build.props", "./"]

RUN dotnet publish "src/AionNetGate.Host/AionNetGate.Host.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# 创建非root用户
RUN groupadd -r aiongate && useradd -r -g aiongate aiongate

# 创建必要的目录
RUN mkdir -p /app/logs /app/data && \
    chown -R aiongate:aiongate /app

# 复制发布文件
COPY --from=build /app/publish .

# 设置环境变量
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    TZ=Asia/Shanghai

# 暴露端口
# 10001: 网关服务端口
# 11001: 管理API端口 (健康检查、Prometheus指标)
EXPOSE 10001 11001

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:11001/health/live || exit 1

# 使用非root用户运行
USER aiongate

# 启动命令
ENTRYPOINT ["dotnet", "AionNetGate.Host.dll"]
