FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Fix for dotnet restore hanging on "Determining projects to restore..."
# See: https://github.com/dotnet/runtime/issues/97828
ENV DOTNET_EnableWriteXorExecute=0

# Copy source and restore+publish in one step (simplest, most reliable)
COPY . .
RUN dotnet restore --verbosity minimal && \
    dotnet publish -c Release -o /app/publish --no-restore --verbosity minimal

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
