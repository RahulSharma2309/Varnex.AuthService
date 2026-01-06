#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${1:-Release}"
KEEP_ENV="${KEEP_ENV:-false}"

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$HERE"

PROJECT_NAME="${COMPOSE_PROJECT_NAME:-varnex-auth-it}"

echo "Publishing integration tests..."
dotnet publish ../test/Varnex.AuthService.Integration.Test/Varnex.AuthService.Integration.Test.csproj -c "$CONFIGURATION" -o ../test/Varnex.AuthService.Integration.Test/obj/docker/publish

echo "Running docker compose integration tests..."
export COMPOSE_PROJECT_NAME="$PROJECT_NAME"

echo "Building docker images explicitly..."
SERVICE_CONTEXT="$HERE/.."
TEST_CONTEXT="$HERE/../test/Varnex.AuthService.Integration.Test"
MOCK_CONTEXT="$TEST_CONTEXT/UserServiceMock"

if [ -n "${GITHUB_TOKEN:-}" ]; then
  docker build -t varnex-auth-service:it --build-arg "GITHUB_TOKEN=$GITHUB_TOKEN" -f "$HERE/Dockerfile" "$SERVICE_CONTEXT"
else
  docker build -t varnex-auth-service:it -f "$HERE/Dockerfile" "$SERVICE_CONTEXT"
fi

docker build -t varnex-auth-user-service-mock:it -f "$MOCK_CONTEXT/Dockerfile.mockserver" "$MOCK_CONTEXT"
docker build -t varnex-auth-service.integration.test:it -f "$TEST_CONTEXT/Dockerfile.test" "$TEST_CONTEXT"

set +e
docker compose -f docker-compose.yml -f docker-compose.test.yml up --build --abort-on-container-exit --exit-code-from auth-service.integration.test
EXIT_CODE=$?
set -e

if [ "$KEEP_ENV" != "true" ]; then
  echo "Cleaning up docker compose environment..."
  docker compose -f docker-compose.yml -f docker-compose.test.yml down -v
fi

exit "$EXIT_CODE"
