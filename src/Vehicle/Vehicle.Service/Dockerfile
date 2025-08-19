# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .sln and .csproj files to restore dependencies
COPY ThreadPilot.sln .
COPY src/Vehicle.Service/Vehicle.Service.csproj ./src/Vehicle.Service/
COPY src/Vehicle.Service.Contracts/Vehicle.Service.Contracts.csproj ./src/Vehicle.Service.Contracts/

# Restore dependencies
RUN dotnet restore ThreadPilot.sln

# Copy the rest of the application code
COPY src/Vehicle.Service ./src/Vehicle.Service
COPY src/Vehicle.Service.Contracts ./src/Vehicle.Service.Contracts

# Build and publish the application
WORKDIR /app/src/Vehicle.Service
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vehicle.Service.dll"]
