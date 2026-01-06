#!/usr/bin/env bash
set -euo pipefail

APP_DLL=${APP_DLL:-Varnex.AuthService.Api.dll}
DB_HOST=${DB_HOST:-mssql}
DB_PORT=${DB_PORT:-1433}
WAIT_TIMEOUT=${WAIT_TIMEOUT:-60}

echo "[entrypoint] waiting for database $DB_HOST:$DB_PORT (timeout ${WAIT_TIMEOUT}s)"
for i in $(seq 1 $WAIT_TIMEOUT); do
  if nc -z "$DB_HOST" "$DB_PORT" >/dev/null 2>&1; then
    echo "[entrypoint] database is available"
    break
  fi
  echo "[entrypoint] waiting for database... ($i)"
  sleep 1
  if [ "$i" -eq "$WAIT_TIMEOUT" ]; then
    echo "[entrypoint] timed out waiting for database"
    exit 1
  fi
done

echo "[entrypoint] starting app: $APP_DLL"
exec dotnet "$APP_DLL"





