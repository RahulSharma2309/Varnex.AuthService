FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .build folder first (needed for Directory.Build.props)
COPY [".build/", ".build/"]

# Copy solution/project files
COPY ["src/Varnex.AuthService.Api/Varnex.AuthService.Api.csproj", "src/Varnex.AuthService.Api/"]
COPY ["src/Varnex.AuthService.Core/Varnex.AuthService.Core.csproj", "src/Varnex.AuthService.Core/"]
COPY ["src/Varnex.AuthService.Abstractions/Varnex.AuthService.Abstractions.csproj", "src/Varnex.AuthService.Abstractions/"]
COPY ["src/Directory.Build.props", "src/"]
COPY ["Varnex.AuthService.sln", "./"]

# Copy nuget.config
COPY ["nuget.config", "./"]

# Accept GitHub token as build argument
ARG GITHUB_TOKEN
# Handle GitHub token and remove local source
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

# Copy everything else
COPY src/ src/

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

# Copy and setup entrypoint
COPY .docker/entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# Environment defaults
ENV APP_DLL=Varnex.AuthService.Api.dll
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
