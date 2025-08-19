# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .sln and .csproj files to restore dependencies
COPY ThreadPilot.sln .
COPY src/Insurance.Service/Insurance.Service.csproj ./src/Insurance.Service/
COPY src/Insurance.Service.Contracts/Insurance.Service.Contracts.csproj ./src/Insurance.Service.Contracts/
COPY src/Vehicle.Service.Contracts/Vehicle.Service.Contracts.csproj ./src/Vehicle.Service.Contracts/

# Restore dependencies
RUN dotnet restore ThreadPilot.sln

# Copy the rest of the application code
COPY src/Insurance.Service ./src/Insurance.Service
COPY src/Insurance.Service.Contracts ./src/Insurance.Service.Contracts
COPY src/Vehicle.Service.Contracts ./src/Vehicle.Service.Contracts

# Build and publish the application
WORKDIR /app/src/Insurance.Service
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Insurance.Service.dll"]
