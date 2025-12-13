FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AlgoDuck/AlgoDuck.csproj", "AlgoDuck/"]
COPY ["AlgoDuckShared/AlgoDuckShared.csproj", "AlgoDuckShared/"]
RUN dotnet restore "AlgoDuck/AlgoDuck.csproj"
COPY AlgoDuckShared/ AlgoDuckShared/
COPY AlgoDuck/ AlgoDuck/
WORKDIR "/src/AlgoDuck"
RUN dotnet build "AlgoDuck.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AlgoDuck.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

USER root
RUN apt-get update && apt-get install -y netcat-openbsd gosu
RUN id app || useradd -m -s /bin/bash app
COPY --from=publish /app/publish .
COPY AlgoDuck/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["sh", "/entrypoint.sh"]

