#!/usr/bin/env bash
#
# Deploys the personal website on a Linux host.
#
# Pulls the image published by the GitHub Actions build, restarts the stack and
# waits until the site answers. Safe to run repeatedly - the database volume is
# never touched, so data survives every deploy.
#
# Usage, from the directory holding docker-compose.yml and .env:
#
#   ./deploy.sh              # pull the latest image and restart
#   ./deploy.sh --no-pull    # restart with the image already on the host
#   ./deploy.sh --logs       # tail the web app log after deploying
#
set -euo pipefail

cd "$(dirname "$(readlink -f "$0")")"

PULL=true
FOLLOW_LOGS=false
for arg in "$@"; do
  case "$arg" in
    --no-pull) PULL=false ;;
    --logs)    FOLLOW_LOGS=true ;;
    -h|--help) sed -n '3,14p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "Unknown option: $arg (try --help)" >&2; exit 1 ;;
  esac
done

step() { printf '\n\033[1m==> %s\033[0m\n' "$1"; }
fail() { printf '\033[31mERROR: %s\033[0m\n' "$1" >&2; exit 1; }

step "Checking prerequisites"
command -v docker >/dev/null || fail "docker is not installed"
docker compose version >/dev/null 2>&1 || fail "the docker compose plugin is not installed"
[ -f docker-compose.yml ] || fail "docker-compose.yml not found in $(pwd)"
[ -f .env ] || fail ".env not found in $(pwd) - copy one of the env.* files from the repository and fill it in"

# Every variable the compose file needs without a default
for required in GITHUB_USER ENVIRONMENT ASPNETCORE_ENVIRONMENT DATABASE_NAME \
                SQL_SERVER_PASSWORD ADMIN_USERNAME ADMIN_PASSWORD; do
  grep -qE "^${required}=.+" .env || fail "$required is missing or empty in .env"
done
echo "docker $(docker version --format '{{.Server.Version}}'), config looks complete"

# The uploads bind mount must exist on the host, otherwise Docker creates it as
# a root-owned directory at container start and the app cannot write into it
step "Ensuring the uploads directory exists"
mkdir -p /appdata/mowebsite/uploads
echo "/appdata/mowebsite/uploads ready"

if [ "$PULL" = true ]; then
  step "Pulling images"
  docker compose pull
fi

step "Starting the stack"
docker compose up -d

step "Waiting for the site to answer"
PORT="$(grep -E '^WEBAPP_PORT=' .env | cut -d= -f2 | tr -d '[:space:]')"
PORT="${PORT:-3001}"
for attempt in $(seq 1 30); do
  code="$(curl -s -o /dev/null -w '%{http_code}' "http://localhost:${PORT}/" || true)"
  if [ "$code" = "200" ]; then
    echo "site answered 200 on port ${PORT} after $((attempt * 5))s at most"
    break
  fi
  if [ "$attempt" = 30 ]; then
    echo
    docker compose ps
    echo
    docker compose logs --tail 40 webapp
    fail "the site did not answer 200 within 150s"
  fi
  sleep 5
done

step "Removing images replaced by this deploy"
docker image prune -f

step "Done"
docker compose ps
echo
ENVIRONMENT_NAME="$(grep -E '^ENVIRONMENT=' .env | cut -d= -f2 | tr -d '[:space:]')"
VOLUME="mo-sqlserver-data-${ENVIRONMENT_NAME:-local}"
echo "Site:     http://$(hostname -I 2>/dev/null | awk '{print $1}'):${PORT}/"
if docker volume inspect "$VOLUME" >/dev/null 2>&1; then
  echo "Database: preserved in volume $VOLUME"
else
  echo "Database: WARNING - volume $VOLUME not found"
fi

if [ "$FOLLOW_LOGS" = true ]; then
  step "Following the web app log (Ctrl+C to stop)"
  docker compose logs -f webapp
fi
