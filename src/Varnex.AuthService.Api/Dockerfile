# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution, NuGet config, and project files for restore
COPY ["Varnex.AuthService.sln", "./"]
COPY ["nuget.config", "./"]
COPY ["src/Varnex.AuthService.Abstractions/Varnex.AuthService.Abstractions.csproj", "src/Varnex.AuthService.Abstractions/"]
COPY ["src/Varnex.AuthService.Core/Varnex.AuthService.Core.csproj", "src/Varnex.AuthService.Core/"]
COPY ["src/Varnex.AuthService.Api/Varnex.AuthService.Api.csproj", "src/Varnex.AuthService.Api/"]
COPY ["test/Varnex.AuthService.Api.Test/Varnex.AuthService.Api.Test.csproj", "test/Varnex.AuthService.Api.Test/"]
COPY ["test/Varnex.AuthService.Core.Test/Varnex.AuthService.Core.Test.csproj", "test/Varnex.AuthService.Core.Test/"]
COPY ["test/Varnex.AuthService.Integration.Test/Varnex.AuthService.Integration.Test.csproj", "test/Varnex.AuthService.Integration.Test/"]
COPY ["Directory.Build.props", "./"]
COPY ["src/Directory.Build.props", "src/"]
COPY ["test/Directory.Build.props", "test/"]
COPY [".build/", ".build/"]

# Pass GitHub token for private packages (Ep.Platform)
ARG GITHUB_TOKEN
ENV GITHUB_TOKEN=${GITHUB_TOKEN}

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish the API project
WORKDIR "/src/src/Varnex.AuthService.Api"
RUN dotnet publish "Varnex.AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Varnex.AuthService.Api.dll"]

