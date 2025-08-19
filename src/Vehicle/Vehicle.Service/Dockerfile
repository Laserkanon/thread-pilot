# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and all project files to restore dependencies
COPY ThreadPilot.sln .
COPY src/ ./src/

# Restore dependencies
RUN dotnet restore ThreadPilot.sln

# Build and publish the application
WORKDIR /app/src/Vehicle/Vehicle.Service
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vehicle.Service.dll"]
