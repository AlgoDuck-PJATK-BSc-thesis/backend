FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 1337

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExecutorService/ExecutorService.csproj", "ExecutorService/"]
RUN dotnet restore "ExecutorService/ExecutorService.csproj"
COPY ExecutorService/ ExecutorService/ 
WORKDIR "/src/ExecutorService"
RUN dotnet build "ExecutorService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExecutorService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    wget \
    unzip \
    git \
    gcc \
    make \
    build-essential \
    linux-headers-amd64 \
    qemu-utils \
    qemu-kvm \
    libvirt-daemon-system \
    libvirt-clients \
    netcat-openbsd \
    bash && \
    apt-get clean
  
RUN wget https://github.com/firecracker-microvm/firecracker/releases/download/v1.2.0/firecracker-v1.2.0-x86_64.tgz && \
    tar -xvf firecracker-v1.2.0-x86_64.tgz && \
    mv release-v1.2.0-x86_64/firecracker-v1.2.0-x86_64 /usr/local/bin/ && \
    mkdir -p /app/fc-scripts && \
    wget https://s3.amazonaws.com/spec.ccfc.min/img/quickstart_guide/x86_64/kernels/vmlinux.bin -O /app/fc-scripts/vmlinux.bin && \
    rm -rf firecracker-v1.2.0-x86_64.tgz && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf release-v1.2.0-x86_64 

RUN wget https://github.com/mikefarah/yq/releases/latest/download/yq_linux_amd64 -O /usr/bin/yq && \
        chmod +x /usr/bin/yq

COPY --from=publish /app/publish .
COPY ExecutorService/Executor/fc-scripts fc-scripts
COPY ExecutorService/Executor/app-scripts app-scripts
COPY ExecutorService/entrypoint.sh .
COPY ExecutorService/ExecutorConfig.yml .
    

RUN    echo '/usr/local/bin/firecracker-v1.2.0-x86_64' >> ~/.bashrc && \
    chmod +x fc-scripts/java-exec.sh  && \
    chmod +x fc-scripts/build-alp.sh && \
    chmod +x fc-scripts/copy-template-fs.sh && \
    chmod +x fc-scripts/populate-execution-fs.sh && \
    chmod +x /app/entrypoint.sh
       
ENTRYPOINT ["/app/entrypoint.sh"]
