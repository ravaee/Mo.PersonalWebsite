# Super simple Dockerfile - just copy pre-built published files
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Copy pre-built published app from GitHub Actions
COPY . .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo.PersonalWebsite.dll"]