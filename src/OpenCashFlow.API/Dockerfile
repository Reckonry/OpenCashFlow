# Multi-stage build for OpenCashFlow.API (.NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj for restore with layer caching
COPY ["src/OpenCashFlow.API/OpenCashFlow.API.csproj", "src/OpenCashFlow.API/"]
COPY ["src/OpenCashFlow.Shared/OpenCashFlow.Shared.csproj", "src/OpenCashFlow.Shared/"]
RUN dotnet restore "src/OpenCashFlow.API/OpenCashFlow.API.csproj"

# Copy the remaining source
COPY . .
RUN dotnet publish "src/OpenCashFlow.API/OpenCashFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenCashFlow.API.dll"]
