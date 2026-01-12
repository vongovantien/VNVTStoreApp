# Use ASP.NET Core Runtime and Build images
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/VNVTStore.API/VNVTStore.API.csproj", "src/VNVTStore.API/"]
COPY ["src/VNVTStore.Application/VNVTStore.Application.csproj", "src/VNVTStore.Application/"]
COPY ["src/VNVTStore.Domain/VNVTStore.Domain.csproj", "src/VNVTStore.Domain/"]
COPY ["src/VNVTStore.Infrastructure/VNVTStore.Infrastructure.csproj", "src/VNVTStore.Infrastructure/"]
RUN dotnet restore "src/VNVTStore.API/VNVTStore.API.csproj"
COPY . .
WORKDIR "/src/src/VNVTStore.API"
RUN dotnet build "VNVTStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VNVTStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VNVTStore.API.dll"]
