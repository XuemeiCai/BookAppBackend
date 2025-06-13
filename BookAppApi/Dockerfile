# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY BookAppApi.csproj ./
RUN dotnet restore BookAppApi.csproj

# Copy the rest of the files and publish
COPY . ./
RUN dotnet publish BookAppApi.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set port for Render (uses PORT environment variable)
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 10000

ENTRYPOINT ["dotnet", "BookAppApi.dll"]
