FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything first to avoid ordering issues in BuildKit context streaming
COPY . .

# Handle GitHub token
ARG GITHUB_TOKEN
RUN sed -i '/<add key="local"/d' nuget.config && \
    if [ -n "$GITHUB_TOKEN" ] && [ "$GITHUB_TOKEN" != "" ]; then \
      sed -i "s|%GITHUB_TOKEN%|$GITHUB_TOKEN|g" nuget.config; \
      echo "GitHub token provided, using GitHub Packages source."; \
    else \
      echo "Warning: GITHUB_TOKEN not provided. Removing GitHub package source and credentials."; \
      sed -i '/<add key="github"/d' nuget.config; \
      sed -i '/<packageSourceCredentials>/,/<\/packageSourceCredentials>/d' nuget.config; \
    fi

# Restore dependencies
RUN dotnet restore "Varnex.AuthService.sln" --no-cache

# Build and publish
WORKDIR "/src/src/Varnex.AuthService.Api"
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Install curl and netcat for healthchecks
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl netcat-openbsd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Setup entrypoint
COPY .docker/entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# Environment defaults
ENV APP_DLL=Varnex.AuthService.Api.dll
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
