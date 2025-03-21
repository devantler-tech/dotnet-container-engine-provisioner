﻿using System.Collections.ObjectModel;
using Devantler.ContainerEngineProvisioner.Core;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Devantler.ContainerEngineProvisioner.Docker;

/// <summary>
/// A provisioner for Docker.
/// </summary>
public sealed class DockerProvisioner : IContainerEngineProvisioner
{
  /// <summary>
  /// The Docker client.
  /// </summary>
  public DockerClient Client { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="DockerProvisioner"/> class.
  /// </summary>
  public DockerProvisioner()
  {
    string? dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
    if (!string.IsNullOrEmpty(dockerHost))
    {
      var uri = new Uri(dockerHost);
      using var uriConfig = new DockerClientConfiguration(uri);
      Client = uriConfig.CreateClient();
      return;
    }
    using var defaultConfig = new DockerClientConfiguration();
    Client = defaultConfig.CreateClient();
  }

  /// <inheritdoc/>
  public async Task<bool> CheckContainerExistsAsync(string name, CancellationToken cancellationToken = default)
  {
    var containers = await Client.Containers.ListContainersAsync(new ContainersListParameters
    {
      All = true,
      Filters = new Dictionary<string, IDictionary<string, bool>>
      {
        ["name"] = new Dictionary<string, bool>
        {
          [$"^{name}$"] = true
        }
      }
    }, cancellationToken).ConfigureAwait(false);

    return containers.Any();
  }

  /// <inheritdoc/>
  public async Task<bool> CheckReadyAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await Client.System.PingAsync(cancellationToken).ConfigureAwait(false);
    }
    catch (DockerApiException)
    {
      return false;
    }
    return true;
  }

  /// <summary>
  /// Creates a directory in a container.
  /// </summary>
  /// <param name="containerId"></param>
  /// <param name="path"></param>
  /// <param name="recursive"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task CreateDirectoryInContainerAsync(string containerId, string path, bool recursive, CancellationToken cancellationToken = default)
  {
    var execResponse = await Client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
    {
      Cmd =
      [
        "mkdir",
        recursive ? "-p" : "",
        path
      ],
    }, cancellationToken).ConfigureAwait(false);
    _ = await Client.Exec.StartAndAttachContainerExecAsync(execResponse.ID, true, cancellationToken).ConfigureAwait(false);
    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Creates a file in a container.
  /// </summary>
  /// <param name="containerId"></param>
  /// <param name="path"></param>
  /// <param name="content"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  public async Task CreateFileInContainerAsync(string containerId, string path, string content, CancellationToken cancellationToken = default)
  {
    var execResponse = await Client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
    {
      Cmd =
      [
        "sh",
        "-c",
        $"echo '{content}' > {path}"
      ]
    }, cancellationToken).ConfigureAwait(false);
    _ = await Client.Exec.StartAndAttachContainerExecAsync(execResponse.ID, true, cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public async Task CreateRegistryAsync(string name, int port, Uri? proxyUrl = default, CancellationToken cancellationToken = default)
  {
    bool registryExists = await CheckContainerExistsAsync(name, cancellationToken).ConfigureAwait(false);

    if (registryExists)
    {
      return;
    }

    await Client.Images.CreateImageAsync(new ImagesCreateParameters
    {
      FromImage = "registry:2"
    }, null, new Progress<JSONMessage>(), cancellationToken).ConfigureAwait(false);

    var createContainerParameters = new CreateContainerParameters
    {
      Image = "registry:2",
      Name = name,
      HostConfig = new HostConfig
      {
        PortBindings = new Dictionary<string, IList<PortBinding>>
        {
          ["5000/tcp"] =
          [
            new() {
              HostPort = $"{port}"
            }
          ]
        },
        RestartPolicy = new RestartPolicy
        {
          Name = RestartPolicyKind.Always
        },
        Binds =
          [
            $"{name}:/var/lib/registry"
          ]
      },
      Env = proxyUrl != null ? new List<string>
      {
        $"REGISTRY_PROXY_REMOTEURL={proxyUrl}"
      } : null
    };
    var registry = await Client.Containers.CreateContainerAsync(createContainerParameters, cancellationToken).ConfigureAwait(false);
    _ = await Client.Containers.StartContainerAsync(registry.ID, new ContainerStartParameters(), cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public async Task CreateRegistryProxyAsync(string name, int port, ReadOnlyCollection<Uri> proxyUrls, CancellationToken cancellationToken = default)
  {
    bool registryExists = await CheckContainerExistsAsync(name, cancellationToken).ConfigureAwait(false);

    if (registryExists)
    {
      return;
    }

    await Client.Images.CreateImageAsync(new ImagesCreateParameters
    {
      FromImage = "rpardini/docker-registry-proxy:0.6.5",
    }, null, new Progress<JSONMessage>(), cancellationToken).ConfigureAwait(false);

    var createContainerParameters = new CreateContainerParameters
    {
      Image = "rpardini/docker-registry-proxy:0.6.5",
      Name = name,
      HostConfig = new HostConfig
      {
        PortBindings = new Dictionary<string, IList<PortBinding>>
        {
          ["3128/tcp"] =
        [
        new() {
          HostPort = $"{port}"
        }
        ]
        },
        Binds =
        [
        $"{name}:/docker_mirror_cache",
        $"{name}_ca:/ca"
        ]
      },
      Env =
      [
      "ENABLE_MANIFEST_CACHE=true",
      $"REGISTRIES={string.Join(" ", proxyUrls.Select(url => url.Host.Contains("docker.io", StringComparison.OrdinalIgnoreCase) ? "docker.io" : url.Host))}",
      ]
    };

    var registry = await Client.Containers.CreateContainerAsync(createContainerParameters, cancellationToken).ConfigureAwait(false);
    _ = await Client.Containers.StartContainerAsync(registry.ID, new ContainerStartParameters(), cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public async Task DeleteRegistryAsync(string name, CancellationToken cancellationToken = default)
  {
    string containerId;
    try
    {
      containerId = await GetContainerIdAsync(name, cancellationToken).ConfigureAwait(false);
    }
    catch (ContainerEngineProvisionerException)
    {
      return;
    }
    _ = await Client.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), cancellationToken).ConfigureAwait(false);
    await Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters(), cancellationToken).ConfigureAwait(false);
  }

  /// <inheritdoc/>
  public async Task<string> GetContainerIdAsync(string name, CancellationToken cancellationToken = default)
  {
    var containers = await Client.Containers.ListContainersAsync(new ContainersListParameters
    {
      All = true,
      Filters = new Dictionary<string, IDictionary<string, bool>>
      {
        ["name"] = new Dictionary<string, bool>
        {
          [$"^{name}$"] = true
        }
      }
    }, cancellationToken).ConfigureAwait(false) ?? throw new ContainerEngineProvisionerException($"Could not find container '{name}'");

    return containers.FirstOrDefault()?.ID ?? throw new ContainerEngineProvisionerException($"Could not find ID for container '{name}'");
  }
}


