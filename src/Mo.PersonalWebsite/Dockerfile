# Use BuildKit cache mounts for fast NuGet restore
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project file first for better Docker layer caching
COPY Mo.PersonalWebsite.csproj .

# Use BuildKit cache mount to persist NuGet packages between builds
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore

# Copy the rest of the source code
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo.PersonalWebsite.dll"]
