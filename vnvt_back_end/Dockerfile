# Use the official .NET 8 SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the solution and project files
COPY vnvt-back-end.sln .
COPY src/vnvt-back-end.API/vnvt-back-end.API.csproj src/vnvt-back-end.API/
COPY src/vnvt-back-end.Application/vnvt-back-end.Application.csproj src/vnvt-back-end.Application/
COPY src/vnvt-back-end.Domain/vnvt-back-end.Domain.csproj src/vnvt-back-end.Domain/
COPY src/vnvt-back-end.Infrastructure/vnvt-back-end.Infrastructure.csproj src/vnvt-back-end.Infrastructure/
COPY src/vnvt-back-end.Tests/vnvt-back-end.Tests.csproj src/vnvt-back-end.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the application code
COPY src/ ./src/

# Build the application
RUN dotnet publish src/vnvt-back-end.API/vnvt-back-end.API.csproj -c Release -o out

# Use the official .NET runtime image as the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build output
COPY --from=build /app/out .

# Expose the port on which the application will run
EXPOSE 80

# Set the entry point for the application
ENTRYPOINT ["dotnet", "vnvt-back-end.API.dll"]
