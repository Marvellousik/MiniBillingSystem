#!/usr/bin/env bash
set -e

echo "========================================="
echo "Starting MiniBillingSystem Stack..."
echo "========================================="

docker compose up -d --build

echo "Waiting for all service healthchecks to pass..."

timeout 120s bash -c '
  until [ "$(docker inspect --format="{{.State.Health.Status}}" minibilling-backend 2>/dev/null)" == "healthy" ] && \
        [ "$(docker inspect --format="{{.State.Health.Status}}" minibilling-frontend 2>/dev/null)" == "healthy" ]; do
    echo -n "."
    sleep 2
  done
'

echo ""
echo "========================================="
echo "SUCCESS: Stack is fully initialized and healthy!"
echo "Frontend: http://localhost:80"
echo "Backend Health: http://localhost:5000/health"
echo "========================================="
docker compose ps
