FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MultiTenantOpenIddictTemplate.sln", "./"]
COPY ["src/Shared/MultiTenancy/MultiTenancy/MultiTenancy.csproj", "src/Shared/MultiTenancy/MultiTenancy/"]
COPY ["src/TenantApi/TenantApi.Domain/TenantApi.Domain.csproj", "src/TenantApi/TenantApi.Domain/"]
COPY ["src/TenantApi/TenantApi.Application/TenantApi.Application.csproj", "src/TenantApi/TenantApi.Application/"]
COPY ["src/TenantApi/TenantApi.Infrastructure/TenantApi.Infrastructure.csproj", "src/TenantApi/TenantApi.Infrastructure/"]
COPY ["src/TenantApi/TenantApi.API/TenantApi.API.csproj", "src/TenantApi/TenantApi.API/"]

# Restore dependencies
RUN dotnet restore "src/TenantApi/TenantApi.API/TenantApi.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/TenantApi/TenantApi.API"
RUN dotnet build "TenantApi.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TenantApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TenantApi.API.dll"]
