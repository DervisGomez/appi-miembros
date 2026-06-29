# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ChurchApi/ChurchApi.csproj", "src/ChurchApi/"]
RUN dotnet restore "src/ChurchApi/ChurchApi.csproj"
COPY ["src/ChurchApi/", "src/ChurchApi/"]
RUN dotnet publish "src/ChurchApi/ChurchApi.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChurchApi.dll"]
