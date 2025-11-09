# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["src/PropertyManagement.Web/PropertyManagement.Web.csproj", "src/PropertyManagement.Web/"]
RUN dotnet restore "src/PropertyManagement.Web/PropertyManagement.Web.csproj"

# Copy the rest of the application code
COPY . .
WORKDIR "/src/src/PropertyManagement.Web"

# Build the application
RUN dotnet build "PropertyManagement.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PropertyManagement.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "PropertyManagement.Web.dll"]
