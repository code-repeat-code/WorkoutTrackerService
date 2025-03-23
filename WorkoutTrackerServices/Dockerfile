# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy .csproj file explicitly
COPY WorkoutTrackerServices/WorkoutTrackerServices.csproj WorkoutTrackerServices/
RUN dotnet restore "WorkoutTrackerServices/WorkoutTrackerServices.csproj"

# Copy full project source
COPY WorkoutTrackerServices/ WorkoutTrackerServices/

WORKDIR "/src/WorkoutTrackerServices"
RUN dotnet build "WorkoutTrackerServices.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WorkoutTrackerServices.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkoutTrackerServices.dll"]