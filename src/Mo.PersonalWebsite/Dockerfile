FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy the entire source for a reliable restore/publish (simpler and robust)
COPY . .

# Restore with cache mount (fast) and then publish (no re-restore)
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore --disable-parallel && \
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

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo.PersonalWebsite.dll"]
