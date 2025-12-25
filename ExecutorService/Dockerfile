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
    bash && \
    apt-get clean
  
RUN wget https://github.com/firecracker-microvm/firecracker/releases/download/v1.2.0/firecracker-v1.2.0-x86_64.tgz && \
    tar -xvf firecracker-v1.2.0-x86_64.tgz && \
    mv release-v1.2.0-x86_64/firecracker-v1.2.0-x86_64 /usr/local/bin/ && \
    mkdir -p /app/firecracker && \
    wget https://s3.amazonaws.com/spec.ccfc.min/img/quickstart_guide/x86_64/kernels/vmlinux.bin -O /app/firecracker/vmlinux.bin && \
    rm -rf firecracker-v1.2.0-x86_64.tgz && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf release-v1.2.0-x86_64 && \
    ln -s /usr/local/bin/firecracker-v1.2.0-x86_64 /usr/local/bin/firecracker && \
    export PATH="/usr/local/bin/firecracker:$PATH"

COPY --from=publish /app/publish .
COPY ExecutorService/Executor/VmScripts firecracker
COPY ExecutorService/VmImages firecracker
    
RUN find firecracker -name "*.sh" -execdir chmod u+x {} +

ENTRYPOINT ["dotnet", "ExecutorService.dll"]
