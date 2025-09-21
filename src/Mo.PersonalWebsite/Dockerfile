FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 1) Copy only project file first to maximize restore layer cache
COPY Mo.PersonalWebsite.csproj ./

# 2) Restore NuGet packages with cache mount
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Mo.PersonalWebsite.csproj --disable-parallel

# 3) Copy the rest of the source
COPY . .

# 4) Publish without restore (uses restored packages)
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish Mo.PersonalWebsite.csproj -c Release -o /app/publish --no-restore

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
