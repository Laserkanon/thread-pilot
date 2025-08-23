## Env variables are provided through kubernetes configmaps and secrets
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ThreadPilot.sln .
COPY src/ ./src/
RUN dotnet restore ThreadPilot.sln
WORKDIR /src/src/Vehicle/Vehicle.Service 
RUN dotnet publish -c ${BUILD_CONFIGURATION} -o /app/publish -p:DebugType=portable -p:DebugSymbols=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS dev
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "Vehicle.Service.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS prod
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "Vehicle.Service.dll"]
