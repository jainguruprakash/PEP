# ===============================
# Stage 1: Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Step 1: Copy solution file
COPY PEPScanner.sln ./
RUN echo "✅ Copied solution file"

# Step 2: Copy individual projects for caching
COPY src/backend/PEPScanner.Domain/*.csproj ./PEPScanner.Domain/
RUN echo "✅ Copied PEPScanner.Domain project"

COPY src/backend/PEPScanner.Infrastructure/*.csproj ./PEPScanner.Infrastructure/
RUN echo "✅ Copied PEPScanner.Infrastructure project"

COPY src/backend/PEPScanner.Application/*.csproj ./PEPScanner.Application/
RUN echo "✅ Copied PEPScanner.Application project"

COPY src/backend/PEPScanner.API/*.csproj ./PEPScanner.API/
RUN echo "✅ Copied PEPScanner.API project"

# Step 3: Restore dependencies
RUN dotnet restore *.sln && echo "✅ dotnet restore completed"

# Step 4: Copy all source files
COPY src/backend/. .
RUN echo "✅ Copied all backend source files"

# Step 5: Publish the API
RUN dotnet publish PEPScanner.API/PEPScanner.API.csproj -c Release -o /app/publish --no-restore && echo "✅ dotnet publish completed"

# ===============================
# Stage 2: Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Step 6: Copy published output
COPY --from=build /app/publish .
RUN echo "✅ Copied published output to runtime image"

# Step 7: Expose port
EXPOSE 5098

# Step 8: Environment variable
ENV ASPNETCORE_URLS=http://+:5098

# Step 9: Entry point
ENTRYPOINT ["dotnet", "PEPScanner.API.dll"]
