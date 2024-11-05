# 🐳 .NET Container Engine Provisioner

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Test](https://github.com/devantler/dotnet-container-engine-provisioner/actions/workflows/test.yaml/badge.svg)](https://github.com/devantler/dotnet-container-engine-provisioner/actions/workflows/test.yaml)
[![codecov](https://codecov.io/gh/devantler/dotnet-container-engine-provisioner/graph/badge.svg?token=RhQPb4fE7z)](https://codecov.io/gh/devantler/dotnet-container-engine-provisioner)

Simple provisioners that can provision various resources in container engines.

<details>
  <summary>Show/hide folder structure</summary>

<!-- readme-tree start -->
```
.
├── .github
│   └── workflows
├── Devantler.ContainerEngineProvisioner.Core
├── Devantler.ContainerEngineProvisioner.Docker
└── Devantler.ContainerEngineProvisioner.Docker.Tests
    └── DockerProvisionerTests

6 directories
```
<!-- readme-tree end -->

</details>

## Prerequisites

- [.NET](https://dotnet.microsoft.com/en-us/)

## 🚀 Getting Started

To get started, you can install the packages from NuGet.

```bash
# For provisioning resources in Docker
dotnet add package Devantler.ContainerEngineProvisioner.Docker
```

## 📝 Usage

To use the provisioners, all you need to do is to create and use a new instance of the provisioner.

```csharp
using Devantler.ContainerEngineProvisioner.Docker;

var provisioner = new DockerProvisioner();

string registryName = "new_registry";
int port = 5010;

// Act
await _provisioner.CreateRegistryAsync(registryName, port, CancellationToken.None);
```
