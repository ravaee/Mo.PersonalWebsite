# Use a more specific base image for better caching
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy only the project file first for better Docker layer caching
COPY Mo.PersonalWebsite.csproj ./

# Copy project files needed for restore (props/targets if any)
COPY ./Properties ./Properties

# Use BuildKit cache mount for NuGet packages to speed up restore (target the csproj directly)
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Mo.PersonalWebsite.csproj --disable-parallel

# Copy the rest of the source code
COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_URLS=http://+:8080

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo.PersonalWebsite.dll"]
