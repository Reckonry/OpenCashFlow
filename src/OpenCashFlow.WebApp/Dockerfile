# Multi-stage build for OpenCashFlow.WebApp (.NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj", "src/OpenCashFlow.WebApp/"]
COPY ["src/OpenCashFlow.Shared/OpenCashFlow.Shared.csproj", "src/OpenCashFlow.Shared/"]
RUN dotnet restore "src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj"

COPY . .
RUN dotnet publish "src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenCashFlow.WebApp.dll"]
