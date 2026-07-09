# Multi-stage build for OpenCashFlow.API (.NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Npgsql can load the native GSSAPI/Kerberos library during PostgreSQL connection setup.
# The ASP.NET runtime image is minimal, so install the small runtime dependency explicitly.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj for restore with layer caching
COPY ["src/OpenCashFlow.API/OpenCashFlow.API.csproj", "src/OpenCashFlow.API/"]
COPY ["src/OpenCashFlow.Contracts/OpenCashFlow.Contracts.csproj", "src/OpenCashFlow.Contracts/"]
COPY ["src/OpenCashFlow.Application/OpenCashFlow.Application.csproj", "src/OpenCashFlow.Application/"]
COPY ["src/OpenCashFlow.Domain/OpenCashFlow.Domain.csproj", "src/OpenCashFlow.Domain/"]
COPY ["src/OpenCashFlow.Infrastructure/OpenCashFlow.Infrastructure.csproj", "src/OpenCashFlow.Infrastructure/"]
RUN dotnet restore "src/OpenCashFlow.API/OpenCashFlow.API.csproj"

# Copy the remaining source
COPY . .
RUN dotnet publish "src/OpenCashFlow.API/OpenCashFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenCashFlow.API.dll"]
