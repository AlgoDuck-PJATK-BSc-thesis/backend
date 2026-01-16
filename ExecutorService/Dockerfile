FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 1337

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExecutorService/ExecutorService.csproj", "ExecutorService/"]
COPY ["AlgoDuckShared/AlgoDuckShared.csproj", "AlgoDuckShared/"]
RUN dotnet restore "ExecutorService/ExecutorService.csproj"

COPY ExecutorService/ ExecutorService/ 
COPY AlgoDuckShared/ AlgoDuckShared/ 

WORKDIR "/src/ExecutorService"
RUN dotnet build "ExecutorService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExecutorService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
WORKDIR /app

RUN apt-get update && \
    if [[ $(uname -a)~="x86_64" ]]; then apt-get install -y --no-install-recommends linux-headers-amd64; fi && \
    apt-get install -y --no-install-recommends \
    curl \
    wget \
    unzip \
    git \
    gcc \
    make \
    build-essential \
    qemu-utils \
    qemu-kvm \
    libvirt-daemon-system \
    libvirt-clients \
    netcat-openbsd \
    procps \
    uuid-runtime \
    jq \
    yq \
    socat \
    bash \
    python3 && \
    apt-get clean
  
 

RUN wget https://s3.amazonaws.com/spec.ccfc.min/firecracker-ci/v1.10/x86_64/firecracker/firecracker-v1.10.1 -O firecracker && \
    mv firecracker /usr/local/bin/firecracker && \
    chmod +x /usr/local/bin/firecracker && \
    mkdir -p /app/firecracker && \
    wget https://s3.amazonaws.com/spec.ccfc.min/firecracker-ci/v1.10/x86_64/vmlinux-6.1.102 -O /app/firecracker/vmlinux.bin && \
    rm -rf /var/lib/apt/lists/* && \
    export PATH="/usr/local/bin/firecracker:$PATH"

RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
    unzip awscliv2.zip && \
    ./aws/install && \
    rm -rf awscliv2.zip aws && \
    rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY ExecutorService/Executor/VmScripts firecracker
#COPY ExecutorService/VmImages firecracker/images
COPY ExecutorService/entrypoint.sh /app/entrypoint.sh

RUN chmod +x /app/entrypoint.sh && \
    find firecracker -name "*.sh" -execdir chmod u+x {} +

ENTRYPOINT ["/app/entrypoint.sh"]