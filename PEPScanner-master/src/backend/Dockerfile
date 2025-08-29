# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file first
COPY *.sln ./

# Copy csproj files for each project to enable layer caching
COPY PEPScanner.API/*.csproj ./PEPScanner.API/
COPY PEPScanner.Domain/*.csproj ./PEPScanner.Domain/
COPY PEPScanner.Infrastructure/*.csproj ./PEPScanner.Infrastructure/
COPY PEPScanner.Application/*.csproj ./PEPScanner.Application/

# Restore dependencies using the solution file
RUN dotnet restore PEPScanner.sln

# Copy everything else
COPY . .

# Publish the API project
RUN dotnet publish PEPScanner.API/PEPScanner.API.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 5098

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:5098

# Entry point
ENTRYPOINT ["dotnet", "PEPScanner.API.dll"]
