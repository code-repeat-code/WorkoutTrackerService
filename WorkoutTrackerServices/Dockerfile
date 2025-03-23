# Use official .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file from the correct relative path
COPY ../WorkoutTrackerServices/WorkoutTrackerServices.csproj ./WorkoutTrackerServices.csproj

# Copy the rest of the source code
COPY ../WorkoutTrackerServices ./WorkoutTrackerServices

# Set working directory for restore/build
WORKDIR /src/WorkoutTrackerServices
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkoutTrackerServices.dll"]
