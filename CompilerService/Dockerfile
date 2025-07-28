FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5137

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CompilerService.csproj", "./"]

RUN dotnet restore "CompilerService.csproj"

COPY . .
RUN dotnet build "CompilerService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CompilerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

USER ROOT

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    bash \
    coreutils \
    netcat-openbsd \
    openjdk-17-jdk-headless && \
    apt-get clean &&\
    mkdir -p app-lib && \
    mkdir -p client-src && \
    mkdir -p client-bytecode && \
    curl -o "/app/app-lib/gson-2.13.1.jar" https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.1/gson-2.13.1.jar

COPY Scripts /app/scripts

# TODO make this not run as root but with write permissions

ENTRYPOINT ["dotnet", "CompilerService.dll"]
