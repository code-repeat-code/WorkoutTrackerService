# Base image with ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image with SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Use the local path for .csproj
COPY ["WorkoutTrackerServices.csproj", "./"]

# Restore dependencies
RUN dotnet restore "WorkoutTrackerServices.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src"
RUN dotnet build "WorkoutTrackerServices.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WorkoutTrackerServices.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkoutTrackerServices.dll"]
