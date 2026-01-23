# Multi-stage build for OpenCashFlow.App (.NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["OpenCashFlow.App/OpenCashFlow.App.csproj", "OpenCashFlow.App/"]
COPY ["OpenCashFlow.Shared/OpenCashFlow.Shared.csproj", "OpenCashFlow.Shared/"]
RUN dotnet restore "OpenCashFlow.App/OpenCashFlow.App.csproj"

COPY . .
WORKDIR "/src/OpenCashFlow.App"
RUN dotnet publish "OpenCashFlow.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenCashFlow.App.dll"]
