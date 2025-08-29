# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file from repo root
COPY PEPScanner.sln ./
RUN echo "✅ Copied solution file"

# Copy project files individually for caching
COPY src/backend/PEPScanner.Domain/*.csproj ./PEPScanner.Domain/
RUN echo "✅ Copied PEPScanner.Domain project"

COPY src/backend/PEPScanner.Infrastructure/*.csproj ./PEPScanner.Infrastructure/
RUN echo "✅ Copied PEPScanner.Infrastructure project"

COPY src/backend/PEPScanner.Application/*.csproj ./PEPScanner.Application/
RUN echo "✅ Copied PEPScanner.Application project"

COPY src/backend/PEPScanner.API/*.csproj ./PEPScanner.API/
RUN echo "✅ Copied PEPScanner.API project"

# Restore dependencies using solution file
RUN dotnet restore *.sln && echo "✅ dotnet restore completed"

# Copy all source files from backend folder
COPY src/backend/. .
RUN echo "✅ Copied all backend source files"

# Publish the API project
RUN dotnet publish PEPScanner.API/PEPScanner.API.csproj -c Release -o /app/publish --no-restore && echo "✅ dotnet publish completed"

# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .
RUN echo "✅ Copied published output to runtime image"

# Expose port
EXPOSE 5098

# ASP.NET Core environment variable
ENV ASPNETCORE_URLS=http://+:5098

ENTRYPOINT ["dotnet", "PEPScanner.API.dll"]
