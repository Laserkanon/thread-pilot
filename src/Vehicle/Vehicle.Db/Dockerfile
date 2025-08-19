# This Dockerfile is for running the DbUp migration project
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Copy solution and all project files to restore dependencies
COPY ThreadPilot.sln .
COPY src/ ./src/

# Restore all dependencies for the solution
RUN dotnet restore ThreadPilot.sln

# Set the working directory to the migration project
WORKDIR /app/src/Vehicle/Vehicle.Db

# The entrypoint will run the migration. The connection string is passed via environment variables.
ENTRYPOINT ["dotnet", "run"]
