#!/bin/bash

# QA Deployment Script for CoachPrime Backend
# This script should be run on the QA server

set -e  # Exit on any error

# Configuration
DEPLOYMENT_DIR="/opt/coachprime-qa"
COMPOSE_FILE="docker-compose.qa.yml"
BACKUP_DIR="/opt/backups/coachprime-qa"
LOG_FILE="/var/log/coachprime-qa-deploy.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
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

# Check if running as root or with sudo
if [[ $EUID -eq 0 ]]; then
    log "${YELLOW}Warning: Running as root. Consider using a non-root user with sudo privileges.${NC}"
fi

# Create deployment directory if it doesn't exist
if [ ! -d "$DEPLOYMENT_DIR" ]; then
    log "Creating deployment directory: $DEPLOYMENT_DIR"
    sudo mkdir -p "$DEPLOYMENT_DIR"
    sudo chown $USER:$USER "$DEPLOYMENT_DIR"
fi

# Create backup directory if it doesn't exist
if [ ! -d "$BACKUP_DIR" ]; then
    log "Creating backup directory: $BACKUP_DIR"
    sudo mkdir -p "$BACKUP_DIR"
    sudo chown $USER:$USER "$BACKUP_DIR"
fi

# Navigate to deployment directory
cd "$DEPLOYMENT_DIR" || error_exit "Cannot navigate to deployment directory"

log "${GREEN}Starting QA deployment...${NC}"

# Backup current docker-compose file if it exists
if [ -f "$COMPOSE_FILE" ]; then
    log "Backing up current docker-compose file..."
    cp "$COMPOSE_FILE" "$BACKUP_DIR/docker-compose.qa.backup.$(date +%Y%m%d_%H%M%S)"
fi

# Stop existing containers
log "Stopping existing containers..."
docker-compose -f "$COMPOSE_FILE" down || log "${YELLOW}No containers to stop${NC}"

# Pull latest image
log "Pulling latest image..."
docker pull ghcr.io/oscarmtz17/coachprime:latest || error_exit "Failed to pull latest image"

# Update docker-compose file with new imagee
log "Updating docker-compose file..."
sed -i "s|image:.*|image: ghcr.io/oscarmtz17/coachprime:latest|" "$COMPOSE_FILE"

# Start containers
log "Starting containers..."
docker-compose -f "$COMPOSE_FILE" up -d || error_exit "Failed to start containers"

# Wait for containers to be healthy
log "Waiting for containers to be healthy..."
sleep 30

# Check container status
log "Checking container status..."
docker-compose -f "$COMPOSE_FILE" ps

# Health check
log "Performing health check..."
for i in {1..10}; do
    if curl -f http://localhost:5000/api/health > /dev/null 2>&1; then
        log "${GREEN}Health check passed!${NC}"
        break
    else
        log "${YELLOW}Health check attempt $i failed, retrying...${NC}"
        sleep 10
    fi
    
    if [ $i -eq 10 ]; then
        error_exit "Health check failed after 10 attempts"
    fi
done

# Show logs
log "Recent container logs:"
docker-compose -f "$COMPOSE_FILE" logs --tail=20

log "${GREEN}QA deployment completed successfully!${NC}"
log "API is available at: http://localhost:5000"
log "Swagger UI: http://localhost:5000/swagger" 