# Installation Guide

> **Platform note:** Due to the use of [Firecracker](https://firecracker-microvm.github.io/) virtualization, the server application requires access to the Linux kernel's KVM module. It can only be run on **Linux** or **WSL2** (Windows Subsystem for Linux 2) on Windows 11 with kernel ≥ 5.10 *(WSL2 is not recommended for long-term use)*.

---

## Table of Contents

- [Hardware Requirements](#hardware-requirements)
- [Prerequisites](#prerequisites)
  - [Kernel Version](#1-kernel-version)
  - [Hardware Virtualization Support](#2-hardware-virtualization-support)
  - [KVM Module](#3-kvm-module)
  - [vhost\_vsock Module](#4-vhost_vsock-module)
- [Installing Dependencies](#installing-dependencies)
  - [System Packages](#1-system-packages)
  - [Docker](#2-docker)
  - [Node.js & npm](#3-nodejs--npm)
- [Setup & Running](#setup--running)
  - [Clone the Repositories](#1-clone-the-repositories)
  - [Configure Environment Variables](#2-configure-environment-variables)
  - [Start the Server](#3-start-the-server)
  - [Start the Client](#4-start-the-client)

---

## Hardware Requirements

| Requirement | Minimum |
|---|---|
| CPU | Intel VT-x or AMD-V virtualization support (must be enabled in BIOS) |
| RAM | 8 GB |
| Disk space | 20 GB free |

> These instructions target **Debian-based** Linux distributions. **Ubuntu 22.04 LTS or newer** is recommended.

---

## Prerequisites

### 1. Kernel Version

Check your kernel version:

```bash
uname -r
```

The minimum required version is **5.10**.

---

### 2. Hardware Virtualization Support

Verify that your CPU supports virtualization extensions:

```bash
egrep -c '(vmx|svm)' /proc/cpuinfo
```

A value **greater than zero** means your CPU is compatible.

---

### 3. KVM Module

Firecracker requires the KVM (Kernel-based Virtual Machine) module. Check if it's loaded:

```bash
lsmod | grep kvm
```

<details>
<summary>Expected output (AMD)</summary>

```
kvm_amd                   208896  0
kvm                      1409024  1 kvm_amd
```

</details>

If the modules are **not** loaded, load them manually:

```bash
sudo modprobe kvm
sudo modprobe kvm_intel  # Intel CPUs
sudo modprobe kvm_amd    # AMD CPUs
```

Then verify the `/dev/kvm` device exists:

```bash
ls -la /dev/kvm
```

---

### 4. vhost_vsock Module

Host ↔ VM communication uses the vsock protocol, which requires the `vhost_vsock` module:

```bash
lsmod | grep vhost_vsock
```

<details>
<summary>Expected output</summary>

```
vhost_vsock            24576  0
vmw_vsock_virtio_transport_common    57344  1 vhost_vsock
vhost                  65536  1 vhost_vsock
vsock                  61440  2 vmw_vsock_virtio_transport_common,vhost_vsock
```

</details>

If not loaded:

```bash
sudo modprobe vhost_vsock
```

---

## Installing Dependencies

### 1. System Packages

Update your package list before proceeding:

```bash
sudo apt update && sudo apt upgrade -y
```

---

### 2. Docker

Docker is used to run the server and its dependencies (PostgreSQL, Redis, RabbitMQ).

Follow the **official Docker installation guide for Ubuntu**:
👉 https://docs.docker.com/engine/install/ubuntu/

---

### 3. Node.js & npm

The client app requires Node.js. Install it via [nvm](https://github.com/nvm-sh/nvm):

```bash
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.0/install.sh | bash
source ~/.bashrc
nvm install v22.17.1
```

---

## Setup & Running

### 1. Clone the Repositories

```bash
git clone https://github.com/AlgoDuck-PJATK-BSc-thesis/backend.git ~/algoduck/backend
git clone https://github.com/AlgoDuck-PJATK-BSc-thesis/frontend.git ~/algoduck/frontend
```

---

### 2. Configure Environment Variables

Create `.env` files for both projects. You can use `nano`:

```bash
nano ~/algoduck/backend/.env
nano ~/algoduck/frontend/.env
```

Or use a heredoc:

```bash
cat > ~/algoduck/backend/.env << EOF
# your env variables here
EOF
```

> See `.env.example` in each repository for the required variables.

---

### 3. Start the Server

The server and all its dependencies are managed via Docker Compose:

```bash
cd ~/algoduck/backend
sudo docker compose up --build -d
```

---

### 4. Start the Client

Install dependencies and start the Vite dev server:

```bash
cd ~/algoduck/frontend
npm install
npm run dev
```

The app will be available at **http://localhost:5173/** (or the address shown in the console).
