# Deployment Setup# Deployment Setup



## Linux Server Setup## Linux Server Setup



### Prerequisites### Prerequisites

1. **Docker and Docker Compose installed on your Linux server**1. **Docker and Docker Compose installed on your Linux server**

   ```bash   ```bash

   # Install Docker   # Install Docker

   curl -fsSL https://get.docker.com -o get-docker.sh   curl -fsSL https://get.docker.com -o get-docker.sh

   sudo sh get-docker.sh   sudo sh get-docker.sh

      

   # Install Docker Compose   # Install Docker Compose

   sudo curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose   sudo curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose

   sudo chmod +x /usr/local/bin/docker-compose   sudo chmod +x /usr/local/bin/docker-compose

   ```   ```



2. **Create deployment directories**2. **Create deployment directories**

   ```bash   ```bash

   sudo mkdir -p /root/mowebsite   sudo mkdir -p /root/mowebsite

   sudo mkdir -p /root/mowebsite-dev   sudo mkdir -p /root/mowebsite-dev

   sudo chown -R $USER:$USER /root/mowebsite*   sudo chown -R $USER:$USER /root/mowebsite*

   ```   ```



3. **Configure SSH access for GitHub Actions**3. **Configure SSH access for GitHub Actions**

   - Generate SSH key pair for GitHub Actions (if not already done)   - Generate SSH key pair for GitHub Actions (if not already done)

   - Add the public key to your server's `~/.ssh/authorized_keys`   - Add the public key to your server's `~/.ssh/authorized_keys`



### GitHub Secrets Setup### GitHub Secrets Setup



Add these secrets to your GitHub repository (Settings > Secrets and variables > Actions):Add these secrets to your GitHub repository (Settings > Secrets and variables > Actions):



| Secret Name | Description | Example || Secret Name | Description | Example |

|-------------|-------------|---------||-------------|-------------|---------|

| `SSH_PRIVATE_KEY` | Private SSH key for server access | `-----BEGIN OPENSSH PRIVATE KEY-----...` || `SSH_PRIVATE_KEY` | Private SSH key for server access | `-----BEGIN OPENSSH PRIVATE KEY-----...` |

| `SSH_USER` | SSH username for your server | `your-username` || `SSH_USER` | SSH username for your server | `your-username` |

| `SERVER_HOST` | Your server IP or domain | `192.168.1.100` or `yourdomain.com` || `SERVER_HOST` | Your server IP or domain | `192.168.1.100` or `yourdomain.com` |

| `WEBAPP_PORT` | Port for the web application | `3000` || `SQL_SERVER_PASSWORD` | Shared SQL Server password | `YourStrongPassword123!` |

| `SQL_SERVER_PASSWORD` | Shared SQL Server password | `YourStrongPassword123!` || `ADMIN_USERNAME` | Admin username for the website | `admin` |

| `ADMIN_USERNAME` | Admin username for the website | `admin` || `ADMIN_PASSWORD` | Admin password for the website | `YourAdminPassword123!` |

| `ADMIN_PASSWORD` | Admin password for the website | `YourAdminPassword123!` |

### Deployment Process

### Deployment Process

1. **Push to `dev` branch** → Deploys to development environment (port 8081)

1. **Push to `dev` branch** → Deploys to development environment2. **Push to `main` branch** → Deploys to production environment (port 80)

2. **Push to `main` branch** → Deploys to production environment

### Environment URLs

### Environment URLs

- **Development**: `http://your-server:8081`

- **Development**: `http://your-server-ip:port` (uses WEBAPP_PORT secret)- **Production**: `http://your-server` (port 80)

- **Production**: `http://your-server-ip:port` (uses WEBAPP_PORT secret)

- **Admin Panel**: `http://your-server-ip:port/admin/login`### Manual Deployment (if needed)



### Port ConfigurationFor local development:

```powershell

The application port is now configurable via the `WEBAPP_PORT` GitHub secret:# Navigate to deployment folder

- If `WEBAPP_PORT` secret is set, it will use that portcd deployment

- If not set, it defaults to port 3000

- Both production and development environments use the same configurable port# Deploy locally

docker-compose --env-file env.local up --build -d

### Accessing Your Application

# Deploy dev environment

After deployment:docker-compose --env-file env.dev up --build -d

```bash```

# Check running containers

docker psFor manual server deployment:

```bash

# Check application logs# Copy the .env file for your environment

docker logs mo-website-prod  # or mo-website-devcp env.prod .env.prod



# Access your website# Edit the .env.prod file with actual values

curl http://localhost:3000  # Replace 3000 with your WEBAPP_PORT valuenano .env.prod

```
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
