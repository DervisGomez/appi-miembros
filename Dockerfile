# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChurchApi.sln", "./"]
COPY ["src/ChurchApi/ChurchApi.csproj", "src/ChurchApi/"]
COPY ["tests/ChurchApi.Tests/ChurchApi.Tests.csproj", "tests/ChurchApi.Tests/"]
RUN dotnet restore "ChurchApi.sln"
COPY . .
RUN dotnet publish "src/ChurchApi/ChurchApi.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChurchApi.dll"]
