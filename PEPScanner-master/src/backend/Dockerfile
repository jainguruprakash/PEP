# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file from repo root
COPY PEPScanner.sln ./

# Copy project files individually for caching
COPY src/backend/PEPScanner.Domain/*.csproj ./PEPScanner.Domain/
COPY src/backend/PEPScanner.Infrastructure/*.csproj ./PEPScanner.Infrastructure/
COPY src/backend/PEPScanner.Application/*.csproj ./PEPScanner.Application/
COPY src/backend/PEPScanner.API/*.csproj ./PEPScanner.API/

# Restore dependencies using the solution
RUN dotnet restore *.sln

# Copy all source files from backend folder
COPY src/backend/. .

# Publish the API project
RUN dotnet publish PEPScanner.API/PEPScanner.API.csproj -c Release -o /app/publish --no-restore

# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port
EXPOSE 5098

# ASP.NET Core environment variable
ENV ASPNETCORE_URLS=http://+:5098

ENTRYPOINT ["dotnet", "PEPScanner.API.dll"]
