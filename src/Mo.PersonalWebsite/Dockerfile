# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Mo.PersonalWebsite.csproj", "Mo.PersonalWebsite/"]
RUN dotnet restore "Mo.PersonalWebsite/Mo.PersonalWebsite.csproj"

# Copy everything else and build
COPY . Mo.PersonalWebsite/
WORKDIR "/src/Mo.PersonalWebsite"
RUN dotnet build "Mo.PersonalWebsite.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Mo.PersonalWebsite.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 80

ENTRYPOINT ["dotnet", "Mo.PersonalWebsite.dll"]
