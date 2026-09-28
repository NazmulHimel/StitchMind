# Stage 1: Base Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

# Stage 2: SDK Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy Solution & Projects for Layer Caching
COPY ["GarmentsERP.slnx", "./"]
COPY ["src/GarmentsERP.Shared/GarmentsERP.Shared.csproj", "src/GarmentsERP.Shared/"]
COPY ["src/GarmentsERP.API/GarmentsERP.API.csproj", "src/GarmentsERP.API/"]

# Copy Modules
COPY ["src/Modules/Auth/GarmentsERP.Auth.Domain/GarmentsERP.Auth.Domain.csproj", "src/Modules/Auth/GarmentsERP.Auth.Domain/"]
COPY ["src/Modules/Auth/GarmentsERP.Auth.Application/GarmentsERP.Auth.Application.csproj", "src/Modules/Auth/GarmentsERP.Auth.Application/"]
COPY ["src/Modules/Auth/GarmentsERP.Auth.Infrastructure/GarmentsERP.Auth.Infrastructure.csproj", "src/Modules/Auth/GarmentsERP.Auth.Infrastructure/"]

COPY ["src/Modules/Finance/GarmentsERP.Finance.Domain/GarmentsERP.Finance.Domain.csproj", "src/Modules/Finance/GarmentsERP.Finance.Domain/"]
COPY ["src/Modules/Finance/GarmentsERP.Finance.Application/GarmentsERP.Finance.Application.csproj", "src/Modules/Finance/GarmentsERP.Finance.Application/"]
COPY ["src/Modules/Finance/GarmentsERP.Finance.Infrastructure/GarmentsERP.Finance.Infrastructure.csproj", "src/Modules/Finance/GarmentsERP.Finance.Infrastructure/"]

COPY ["src/Modules/HR/GarmentsERP.HR.Domain/GarmentsERP.HR.Domain.csproj", "src/Modules/HR/GarmentsERP.HR.Domain/"]
COPY ["src/Modules/HR/GarmentsERP.HR.Application/GarmentsERP.HR.Application.csproj", "src/Modules/HR/GarmentsERP.HR.Application/"]
COPY ["src/Modules/HR/GarmentsERP.HR.Infrastructure/GarmentsERP.HR.Infrastructure.csproj", "src/Modules/HR/GarmentsERP.HR.Infrastructure/"]

COPY ["src/Modules/Inventory/GarmentsERP.Inventory.Domain/GarmentsERP.Inventory.Domain.csproj", "src/Modules/Inventory/GarmentsERP.Inventory.Domain/"]
COPY ["src/Modules/Inventory/GarmentsERP.Inventory.Application/GarmentsERP.Inventory.Application.csproj", "src/Modules/Inventory/GarmentsERP.Inventory.Application/"]
COPY ["src/Modules/Inventory/GarmentsERP.Inventory.Infrastructure/GarmentsERP.Inventory.Infrastructure.csproj", "src/Modules/Inventory/GarmentsERP.Inventory.Infrastructure/"]

COPY ["src/Modules/Procurement/GarmentsERP.Procurement.Domain/GarmentsERP.Procurement.Domain.csproj", "src/Modules/Procurement/GarmentsERP.Procurement.Domain/"]
COPY ["src/Modules/Procurement/GarmentsERP.Procurement.Application/GarmentsERP.Procurement.Application.csproj", "src/Modules/Procurement/GarmentsERP.Procurement.Application/"]
COPY ["src/Modules/Procurement/GarmentsERP.Procurement.Infrastructure/GarmentsERP.Procurement.Infrastructure.csproj", "src/Modules/Procurement/GarmentsERP.Procurement.Infrastructure/"]

COPY ["src/Modules/Production/GarmentsERP.Production.Domain/GarmentsERP.Production.Domain.csproj", "src/Modules/Production/GarmentsERP.Production.Domain/"]
COPY ["src/Modules/Production/GarmentsERP.Production.Application/GarmentsERP.Production.Application.csproj", "src/Modules/Production/GarmentsERP.Production.Application/"]
COPY ["src/Modules/Production/GarmentsERP.Production.Infrastructure/GarmentsERP.Production.Infrastructure.csproj", "src/Modules/Production/GarmentsERP.Production.Infrastructure/"]

COPY ["src/Modules/Reporting/GarmentsERP.Reporting.Application/GarmentsERP.Reporting.Application.csproj", "src/Modules/Reporting/GarmentsERP.Reporting.Application/"]
COPY ["src/Modules/Reporting/GarmentsERP.Reporting.Infrastructure/GarmentsERP.Reporting.Infrastructure.csproj", "src/Modules/Reporting/GarmentsERP.Reporting.Infrastructure/"]

# Restore packages
RUN dotnet restore "src/GarmentsERP.API/GarmentsERP.API.csproj"

# Copy all source files
COPY . .
WORKDIR "/src/src/GarmentsERP.API"
RUN dotnet build "GarmentsERP.API.csproj" -c Release -o /app/build

# Stage 3: Publish Trimmed / Optimized Artifact
FROM build AS publish
RUN dotnet publish "GarmentsERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Production Runner Container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user for security hardening
USER $APP_UID
ENTRYPOINT ["dotnet", "GarmentsERP.API.dll"]
