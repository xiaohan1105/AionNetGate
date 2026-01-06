#!/bin/bash
# AionNetGate 快速部署脚本
# 用法: ./scripts/deploy.sh [命令]
#
# 命令:
#   start     - 启动所有服务
#   stop      - 停止所有服务
#   restart   - 重启所有服务
#   status    - 查看服务状态
#   logs      - 查看日志
#   monitor   - 启动带监控的完整服务
#   build     - 重新构建镜像

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查环境变量文件
check_env() {
    if [ ! -f ".env" ]; then
        log_warn ".env 文件不存在，从示例文件创建..."
        if [ -f ".env.example" ]; then
            cp .env.example .env
            log_warn "请编辑 .env 文件设置正确的密钥和密码"
            exit 1
        else
            log_error "未找到 .env.example 文件"
            exit 1
        fi
    fi
}

case "${1:-start}" in
    start)
        log_info "启动 AionNetGate 服务..."
        check_env
        docker-compose up -d gateway mysql
        log_info "服务已启动"
        log_info "网关端口: 10001"
        log_info "管理API: http://localhost:11001"
        log_info "健康检查: http://localhost:11001/health"
        ;;

    stop)
        log_info "停止 AionNetGate 服务..."
        docker-compose down
        log_info "服务已停止"
        ;;

    restart)
        log_info "重启 AionNetGate 服务..."
        docker-compose restart
        log_info "服务已重启"
        ;;

    status)
        log_info "服务状态:"
        docker-compose ps
        echo ""
        log_info "健康检查:"
        curl -s http://localhost:11001/health | jq . 2>/dev/null || echo "服务未运行或无法访问"
        ;;

    logs)
        docker-compose logs -f ${2:-gateway}
        ;;

    monitor)
        log_info "启动完整服务 (包含监控)..."
        check_env
        docker-compose --profile monitoring up -d
        log_info "服务已启动"
        log_info "网关端口: 10001"
        log_info "管理API: http://localhost:11001"
        log_info "Prometheus: http://localhost:9090"
        log_info "Grafana: http://localhost:3000 (admin/admin123)"
        ;;

    build)
        log_info "构建 Docker 镜像..."
        docker-compose build --no-cache
        log_info "构建完成"
        ;;

    *)
        echo "AionNetGate 部署脚本"
        echo ""
        echo "用法: $0 [命令]"
        echo ""
        echo "命令:"
        echo "  start     启动基础服务 (网关 + MySQL)"
        echo "  stop      停止所有服务"
        echo "  restart   重启所有服务"
        echo "  status    查看服务状态"
        echo "  logs      查看日志 (可选: logs gateway|mysql|prometheus)"
        echo "  monitor   启动完整服务 (包含 Prometheus + Grafana)"
        echo "  build     重新构建 Docker 镜像"
        ;;
esac
