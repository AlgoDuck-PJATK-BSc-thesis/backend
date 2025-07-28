FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7139

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DockerWarden/DockerWarden.csproj", "DockerWarden/"]
RUN dotnet restore "DockerWarden/DockerWarden.csproj"
COPY . .
WORKDIR "/src/DockerWarden"
RUN dotnet build "./DockerWarden.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DockerWarden.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .
COPY DockerWarden/Scripts/ /app/Scripts/

RUN chmod +x /app/Scripts/deploy-compiler.sh && \
    chmod +x /app/Scripts/kill-compiler.sh

ENTRYPOINT ["dotnet", "DockerWarden.dll"]
