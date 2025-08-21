# syntax=docker/dockerfile:1
# -------- build stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution + sources
COPY ThreadPilot.sln .
COPY src/ ./src/

# restore once at the solution level
RUN dotnet restore ThreadPilot.sln

# publish the Insurance service
WORKDIR /src/src/Insurance/Insurance.Service
RUN dotnet publish -c Release -o /app/publish

# -------- dev image (for docker-compose local) --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS dev
WORKDIR /app
# Add curl so your healthchecks work
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "Insurance.Service.dll"]

# -------- production image --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS prod
WORKDIR /app
COPY --from=build /app/publish .
# Bind to all interfaces in container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "Insurance.Service.dll"]
