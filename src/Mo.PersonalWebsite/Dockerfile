# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (this layer will be cached if deps don't change)
COPY ["Mo.PersonalWebsite.csproj", "Mo.PersonalWebsite/"]
RUN dotnet restore "Mo.PersonalWebsite/Mo.PersonalWebsite.csproj"

# Copy everything else and build
COPY . Mo.PersonalWebsite/
WORKDIR "/src/Mo.PersonalWebsite"

# Build with optimizations
RUN dotnet build "Mo.PersonalWebsite.csproj" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "Mo.PersonalWebsite.csproj" -c Release -o /app/publish --no-restore --no-build /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create uploads directory with proper permissions
RUN mkdir -p /app/wwwroot/uploads && \
    chown -R app:app /app/wwwroot 2>/dev/null || true && \
    chmod -R 755 /app/wwwroot 2>/dev/null || true

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo.PersonalWebsite.dll"]
