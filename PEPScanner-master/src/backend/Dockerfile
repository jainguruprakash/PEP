FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.sln ./
COPY PEPScanner.API/*.csproj ./PEPScanner.API/
COPY PEPScanner.Domain/*.csproj ./PEPScanner.Domain/
COPY PEPScanner.Infrastructure/*.csproj ./PEPScanner.Infrastructure/
COPY PEPScanner.Application/*.csproj ./PEPScanner.Application/
RUN dotnet restore
COPY . .
RUN dotnet publish PEPScanner.API/PEPScanner.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5098
ENV ASPNETCORE_URLS=http://+:5098
ENTRYPOINT ["dotnet", "PEPScanner.API.dll"]