FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MultiTenantOpenIddictTemplate.sln", "./"]
COPY ["src/Shared/MultiTenancy/MultiTenancy/MultiTenancy.csproj", "src/Shared/MultiTenancy/MultiTenancy/"]
COPY ["src/AuthServer/AuthServer.Domain/AuthServer.Domain.csproj", "src/AuthServer/AuthServer.Domain/"]
COPY ["src/AuthServer/AuthServer.Application/AuthServer.Application.csproj", "src/AuthServer/AuthServer.Application/"]
COPY ["src/AuthServer/AuthServer.Infrastructure/AuthServer.Infrastructure.csproj", "src/AuthServer/AuthServer.Infrastructure/"]
COPY ["src/AuthServer/AuthServer.API/AuthServer.API.csproj", "src/AuthServer/AuthServer.API/"]

# Restore dependencies
RUN dotnet restore "src/AuthServer/AuthServer.API/AuthServer.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/AuthServer/AuthServer.API"
RUN dotnet build "AuthServer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthServer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthServer.API.dll"]
