# ===========================================
# AionGate 2.0 Docker镜像
# ===========================================
# 多阶段构建，优化镜像大小

# 阶段1: 构建
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 复制项目文件并还原依赖
COPY ["src/AionGate.Core/AionGate.Core.csproj", "AionGate.Core/"]
COPY ["src/AionGate.Gateway/AionGate.Gateway.csproj", "AionGate.Gateway/"]
COPY ["src/AionGate.Auth/AionGate.Auth.csproj", "AionGate.Auth/"]
COPY ["src/AionGate.Defense/AionGate.Defense.csproj", "AionGate.Defense/"]
COPY ["src/AionGate.Monitor/AionGate.Monitor.csproj", "AionGate.Monitor/"]
COPY ["src/AionGate.Data/AionGate.Data.csproj", "AionGate.Data/"]
COPY ["src/AionGate.Infrastructure/AionGate.Infrastructure.csproj", "AionGate.Infrastructure/"]
COPY ["src/AionGate.Server/AionGate.Server.csproj", "AionGate.Server/"]

RUN dotnet restore "AionGate.Server/AionGate.Server.csproj"

# 复制源代码并构建
COPY ["src/", "."]
RUN dotnet publish "AionGate.Server/AionGate.Server.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false

# 阶段2: 运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# 安装必要的运行时依赖
RUN apk add --no-cache \
    ca-certificates \
    tzdata \
    icu-libs \
    libgcc \
    libstdc++ \
    zlib \
    wget \
    && rm -rf /var/cache/apk/*

# 设置时区
ENV TZ=Asia/Shanghai

# 创建非特权用户
RUN addgroup -g 1000 aiongate && \
    adduser -u 1000 -G aiongate -s /bin/sh -D aiongate

# 创建必要目录
RUN mkdir -p /app/logs /app/data /app/config && \
    chown -R aiongate:aiongate /app

# 复制构建产物
COPY --from=build --chown=aiongate:aiongate /app/publish .

# 复制默认配置
COPY --chown=aiongate:aiongate config/appsettings.yaml /app/config/

# 切换到非特权用户
USER aiongate

# 暴露端口
EXPOSE 10001/tcp
EXPOSE 9090/tcp
EXPOSE 9091/tcp

# 环境变量
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:9090

# 健康检查
HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD wget -qO- http://localhost:9090/health || exit 1

# 启动命令
ENTRYPOINT ["dotnet", "AionGate.Server.dll"]
