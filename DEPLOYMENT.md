# CoachPrime Backend - QA Deployment Guide

## Overview

This document describes how to deploy the CoachPrime backend to the QA environment using CI/CD with GitHub Actions and DigitalOcean.

## Prerequisites

### 1. DigitalOcean Droplet Setup

- **OS**: Ubuntu 22.04 LTS
- **Size**: At least 2GB RAM, 1 vCPU
- **Storage**: At least 50GB SSD

### 2. Required Software on Droplet

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Install curl for health checks
sudo apt install curl -y

# Logout and login again for docker group to take effect
```

### 3. Create Deployment User (Optional but Recommended)

```bash
# Create a new user for deployments
sudo adduser deploy
sudo usermod -aG docker deploy
sudo usermod -aG sudo deploy

# Switch to deploy user
su - deploy
```

### 4. Setup SSH Key Authentication

```bash
# On your local machine, generate SSH key if you don't have one
ssh-keygen -t rsa -b 4096 -C "your-email@example.com"

# Copy public key to server
ssh-copy-id deploy@your-server-ip

# Test SSH connection
ssh deploy@your-server-ip
```

## GitHub Repository Setup

### 1. Enable GitHub Packages

- Go to your repository settings
- Navigate to "Packages" section
- Ensure "Inherit access from source repository" is enabled

### 2. Configure GitHub Secrets

Go to your repository → Settings → Secrets and variables → Actions, and add:

| Secret Name   | Description                  | Example Value                            |
| ------------- | ---------------------------- | ---------------------------------------- |
| `QA_HOST`     | IP address of your QA server | `123.456.789.012`                        |
| `QA_USERNAME` | SSH username                 | `deploy`                                 |
| `QA_SSH_KEY`  | Private SSH key content      | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `QA_PORT`     | SSH port (usually 22)        | `22`                                     |

### 3. Create Environment Protection Rules

- Go to Settings → Environments
- Create environment named `qa`
- Add protection rules if needed (branch restrictions, required reviewers)

## Deployment Process

### 1. Automatic Deployment

The deployment happens automatically when:

- Code is pushed to `qa` branch (deploys to QA environment)
- Code is pushed to `main` branch (deploys to Production environment)
- Manual trigger via GitHub Actions UI

### 2. Manual Deployment

```bash
# SSH to your QA server
ssh deploy@your-server-ip

# Navigate to deployment directory
cd /opt/coachprime-qa

# Run deployment script
./scripts/deploy-qa.sh
```

### 3. Verify Deployment

```bash
# Check container status
docker-compose -f docker-compose.qa.yml ps

# Check logs
docker-compose -f docker-compose.qa.yml logs -f

# Test health endpoint
curl http://localhost:5000/api/health
```

## Configuration Files

### 1. Environment Variables

Copy `env.qa.example` to `env.qa` and update values:

```bash
cp env.qa.example env.qa
nano env.qa
```

### 2. Docker Compose

The `docker-compose.qa.yml` file is automatically updated by the CI/CD pipeline.

## Monitoring and Logs

### 1. View Logs

```bash
# All containers
docker-compose -f docker-compose.qa.yml logs

# Specific service
docker-compose -f docker-compose.qa.yml logs webapi

# Follow logs in real-time
docker-compose -f docker-compose.qa.yml logs -f webapi
```

### 2. Health Checks

- **Endpoint**: `http://your-server-ip:5000/api/health`
- **Ready Check**: `http://your-server-ip:5000/api/health/ready`

### 3. Monitoring Commands

```bash
# Check resource usage
docker stats

# Check disk space
df -h

# Check memory usage
free -h

# Check running processes
docker ps
```

## Troubleshooting

### 1. Container Won't Start

```bash
# Check container logs
docker-compose -f docker-compose.qa.yml logs webapi

# Check if port is in use
sudo netstat -tlnp | grep :5000

# Restart containers
docker-compose -f docker-compose.qa.yml restart
```

### 2. Database Connection Issues

```bash
# Check SQL Server logs
docker-compose -f docker-compose.qa.yml logs sqlserver

# Test database connection
docker-compose -f docker-compose.qa.yml exec webapi dotnet ef database update
```

### 3. Health Check Fails

```bash
# Check if API is responding
curl -v http://localhost:5000/api/health

# Check container health status
docker-compose -f docker-compose.qa.yml ps
```

## Backup and Recovery

### 1. Database Backup

```bash
# Create backup
docker-compose -f docker-compose.qa.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "BACKUP DATABASE CoachPrimeDB TO DISK = '/var/opt/mssql/backup/CoachPrimeDB_$(date +%Y%m%d_%H%M%S).bak'"

# Restore from backup
docker-compose -f docker-compose.qa.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "RESTORE DATABASE CoachPrimeDB FROM DISK = '/var/opt/mssql/backup/your_backup_file.bak'"
```

### 2. Configuration Backup

```bash
# Backup docker-compose file
cp docker-compose.qa.yml /opt/backups/coachprime-qa/

# Backup environment variables
cp env.qa /opt/backups/coachprime-qa/
```

## Security Considerations

### 1. Firewall Configuration

```bash
# Allow only necessary ports
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 5000/tcp  # API
sudo ufw enable
```

### 2. SSL/TLS (Optional)

For production, consider adding SSL/TLS with Let's Encrypt or a reverse proxy like Nginx.

### 3. Regular Updates

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Update Docker images
docker-compose -f docker-compose.qa.yml pull
```

## Rollback Procedure

### 1. Quick Rollback

```bash
# Stop current containers
docker-compose -f docker-compose.qa.yml down

# Pull previous image
docker pull ghcr.io/your-username/your-repo:previous-tag

# Update docker-compose with previous image
sed -i 's|image:.*|image: ghcr.io/your-username/your-repo:previous-tag|' docker-compose.qa.yml

# Start containers
docker-compose -f docker-compose.qa.yml up -d
```

### 2. Database Rollback

```bash
# Restore from backup
docker-compose -f docker-compose.qa.yml exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "RESTORE DATABASE CoachPrimeDB FROM DISK = '/var/opt/mssql/backup/backup_file.bak'"
```

## Support and Maintenance

### 1. Regular Maintenance Tasks

- Monitor disk space usage
- Check container resource usage
- Review application logs for errors
- Update system packages monthly
- Test backup and restore procedures

### 2. Contact Information

For deployment issues, contact the development team or check the GitHub repository issues.

## Next Steps

1. **Production Deployment**: Similar setup for production environment
2. **Monitoring**: Add application monitoring (e.g., Prometheus, Grafana)
3. **Logging**: Centralized logging solution (e.g., ELK Stack)
4. **SSL/TLS**: Add HTTPS support
5. **Load Balancing**: Multiple instances behind a load balancer
