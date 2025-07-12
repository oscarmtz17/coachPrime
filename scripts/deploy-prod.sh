#!/bin/bash

# Production Deployment Script for CoachPrime Backend
# This script should be run on the production server

set -e  # Exit on any error

# Configuration
DEPLOYMENT_DIR="/opt/coachprime-prod"
COMPOSE_FILE="docker-compose.prod.yml"
BACKUP_DIR="/opt/backups/coachprime-prod"
LOG_FILE="/var/log/coachprime-prod-deploy.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

# Error handling
error_exit() {
    log "${RED}ERROR: $1${NC}"
    exit 1
}

# Warning function
warning() {
    log "${YELLOW}WARNING: $1${NC}"
}

# Success function
success() {
    log "${GREEN}SUCCESS: $1${NC}"
}

# Info function
info() {
    log "${BLUE}INFO: $1${NC}"
}

# Check if running as root or with sudo
if [[ $EUID -eq 0 ]]; then
    warning "Running as root. Consider using a non-root user with sudo privileges."
fi

# Create deployment directory if it doesn't exist
if [ ! -d "$DEPLOYMENT_DIR" ]; then
    info "Creating deployment directory: $DEPLOYMENT_DIR"
    sudo mkdir -p "$DEPLOYMENT_DIR"
    sudo chown $USER:$USER "$DEPLOYMENT_DIR"
fi

# Create backup directory if it doesn't exist
if [ ! -d "$BACKUP_DIR" ]; then
    info "Creating backup directory: $BACKUP_DIR"
    sudo mkdir -p "$BACKUP_DIR"
    sudo chown $USER:$USER "$BACKUP_DIR"
fi

# Navigate to deployment directory
cd "$DEPLOYMENT_DIR" || error_exit "Cannot navigate to deployment directory"

info "Starting production deployment..."

# Pre-deployment checks
info "Performing pre-deployment checks..."

# Check disk space
DISK_USAGE=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
if [ "$DISK_USAGE" -gt 90 ]; then
    error_exit "Disk usage is too high: ${DISK_USAGE}%"
fi

# Check memory
MEMORY_USAGE=$(free | grep Mem | awk '{printf("%.0f", $3/$2 * 100.0)}')
if [ "$MEMORY_USAGE" -gt 90 ]; then
    warning "Memory usage is high: ${MEMORY_USAGE}%"
fi

# Backup current docker-compose file if it exists
if [ -f "$COMPOSE_FILE" ]; then
    info "Backing up current docker-compose file..."
    cp "$COMPOSE_FILE" "$BACKUP_DIR/docker-compose.prod.backup.$(date +%Y%m%d_%H%M%S)"
fi

# Backup database before deployment
info "Creating database backup..."
if docker-compose -f "$COMPOSE_FILE" ps | grep -q "Up"; then
    docker-compose -f "$COMPOSE_FILE" exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "BACKUP DATABASE CoachPrimeDB TO DISK = '/var/opt/mssql/backup/CoachPrimeDB_pre_deploy_$(date +%Y%m%d_%H%M%S).bak'" || warning "Database backup failed, continuing anyway..."
fi

# Stop existing containers gracefully
info "Stopping existing containers..."
docker-compose -f "$COMPOSE_FILE" down || warning "No containers to stop"

# Pull latest image
info "Pulling latest production image..."
docker pull ghcr.io/your-username/your-repo:latest || error_exit "Failed to pull latest image"

# Update docker-compose file with new image
info "Updating docker-compose file..."
# This will be done by the GitHub Actions workflow

# Start containers
info "Starting containers..."
docker-compose -f "$COMPOSE_FILE" up -d || error_exit "Failed to start containers"

# Wait for containers to be healthy
info "Waiting for containers to be healthy..."
sleep 45

# Check container status
info "Checking container status..."
docker-compose -f "$COMPOSE_FILE" ps

# Health check with retries
info "Performing health check..."
HEALTH_CHECK_PASSED=false
for i in {1..15}; do
    if curl -f http://localhost:5000/api/health > /dev/null 2>&1; then
        success "Health check passed on attempt $i!"
        HEALTH_CHECK_PASSED=true
        break
    else
        warning "Health check attempt $i failed, retrying..."
        sleep 10
    fi
done

if [ "$HEALTH_CHECK_PASSED" = false ]; then
    error_exit "Health check failed after 15 attempts"
fi

# Additional production checks
info "Performing additional production checks..."

# Check if database migrations are needed
info "Checking for pending database migrations..."
docker-compose -f "$COMPOSE_FILE" exec webapi dotnet ef migrations list || warning "Could not check migrations"

# Check resource usage
info "Checking resource usage..."
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Show recent logs
info "Recent container logs:"
docker-compose -f "$COMPOSE_FILE" logs --tail=20

success "Production deployment completed successfully!"
info "API is available at: http://localhost:5000"
info "Swagger UI: http://localhost:5000/swagger"
info "Health endpoint: http://localhost:5000/api/health"

# Post-deployment notification (you can add Slack, email, etc.)
info "Sending deployment notification..."
# Add your notification logic here
# Example: curl -X POST -H 'Content-type: application/json' --data '{"text":"Production deployment completed successfully!"}' YOUR_SLACK_WEBHOOK_URL 