FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7139

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SystemWarden/SystemWarden.csproj", "SystemWarden/"]
RUN dotnet restore "SystemWarden/SystemWarden.csproj"
COPY . .
WORKDIR "/src/SystemWarden"
RUN dotnet build "./SystemWarden.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SystemWarden.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .
COPY SystemWarden/Scripts/ /app/Scripts/

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    unzip \
    bash && \
    apt-get clean

RUN chmod +x /app/Scripts/deploy-compiler.sh && \
    chmod +x /app/Scripts/kill-compiler.sh && \
    chmod +x /app/Scripts/build-alp.sh  && \
    chmod +x /app/Scripts/create-and-mount-fs.sh && \
    chmod +x /app/Scripts/entrypoint.sh && \
    chmod +x /app/Scripts/umount.sh

ENTRYPOINT ["/app/Scripts/entrypoint.sh"]
