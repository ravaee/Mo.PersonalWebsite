# Deployment Setup

## Linux Server Setup

### Prerequisites
1. **Docker and Docker Compose installed on your Linux server**
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   
   # Install Docker Compose
   sudo curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
   sudo chmod +x /usr/local/bin/docker-compose
   ```

2. **Create deployment directories**
   ```bash
   sudo mkdir -p /opt/mo-website-prod
   sudo mkdir -p /opt/mo-website-dev
   sudo chown -R $USER:$USER /opt/mo-website-*
   ```

3. **Configure SSH access for GitHub Actions**
   - Generate SSH key pair for GitHub Actions (if not already done)
   - Add the public key to your server's `~/.ssh/authorized_keys`

### GitHub Secrets Setup

Add these secrets to your GitHub repository (Settings > Secrets and variables > Actions):

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SSH_PRIVATE_KEY` | Private SSH key for server access | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `SSH_USER` | SSH username for your server | `your-username` |
| `SERVER_HOST` | Your server IP or domain | `192.168.1.100` or `yourdomain.com` |
| `SQL_SERVER_PASSWORD` | Shared SQL Server password | `YourStrongPassword123!` |
| `ADMIN_USERNAME` | Admin username for the website | `admin` |
| `ADMIN_PASSWORD` | Admin password for the website | `YourAdminPassword123!` |

### Deployment Process

1. **Push to `dev` branch** → Deploys to development environment (port 8081)
2. **Push to `main` branch** → Deploys to production environment (port 80)

### Environment URLs

- **Development**: `http://your-server:8081`
- **Production**: `http://your-server` (port 80)

### Manual Deployment (if needed)

For local development:
```powershell
# Navigate to deployment folder
cd deployment

# Deploy locally
docker-compose --env-file env.local up --build -d

# Deploy dev environment
docker-compose --env-file env.dev up --build -d
```

For manual server deployment:
```bash
# Copy the .env file for your environment
cp env.prod .env.prod

# Edit the .env.prod file with actual values
nano .env.prod

# Deploy
docker-compose --env-file .env.prod up --build -d

# Clean up
rm .env.prod
```

### Database Access

Each environment uses its own database:
- **Local**: `MoPersonalWebsite_Local`
- **Dev**: `MoPersonalWebsite_Dev` 
- **Prod**: `MoPersonalWebsite_Prod`

All environments share the same SQL Server instance but with different database names.
