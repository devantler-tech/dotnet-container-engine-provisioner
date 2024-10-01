using Devantler.ContainerEngineProvisioner.Core;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Devantler.ContainerEngineProvisioner.Docker;

/// <summary>
/// A provisioner for Docker.
/// </summary>
public sealed class DockerProvisioner : IContainerEngineProvisioner
{
  readonly DockerClient _dockerClient;

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
      _dockerClient = uriConfig.CreateClient();
      return;
    }
    using var defaultConfig = new DockerClientConfiguration();
    _dockerClient = defaultConfig.CreateClient();
  }

  /// <inheritdoc/>
  public async Task<bool> CheckContainerExistsAsync(string name, CancellationToken token)
  {
    var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters
    {
      All = true,
      Filters = new Dictionary<string, IDictionary<string, bool>>
      {
        ["name"] = new Dictionary<string, bool>
        {
          [name] = true
        }
      }
    }, token).ConfigureAwait(false);

    return containers.Any();
  }

  /// <inheritdoc/>
  public async Task<bool> CheckReadyAsync(CancellationToken token)
  {
    try
    {
      await _dockerClient.System.PingAsync(token).ConfigureAwait(false);
    }
    catch (DockerApiException)
    {
      return false;
    }
    return true;
  }

  /// <inheritdoc/>
  public async Task CreateRegistryAsync(string name, int port, CancellationToken token, Uri? proxyUrl = null)
  {
    bool registryExists = await CheckContainerExistsAsync(name, token).ConfigureAwait(false);

    if (registryExists)
    {
      return;
    }
    CreateContainerResponse registry;
    try
    {
      await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
      {
        FromImage = "registry:2"
      }, null, new Progress<JSONMessage>()).ConfigureAwait(false);
      registry = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
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
      }).ConfigureAwait(false);
      _ = await _dockerClient.Containers.StartContainerAsync(registry.ID, new ContainerStartParameters()).ConfigureAwait(false);
    }
    catch (DockerApiException)
    {
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task DeleteRegistryAsync(string name, CancellationToken token)
  {
    string containerId = await GetContainerIdAsync(name, token).ConfigureAwait(false);

    if (string.IsNullOrEmpty(containerId))
    {
      throw new InvalidOperationException($"Could not find registry '{name}'");
    }
    else
    {
      _ = await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), token).ConfigureAwait(false);
      await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters(), token).ConfigureAwait(false);
    }
  }

  /// <inheritdoc/>
  public async Task<string> GetContainerIdAsync(string name, CancellationToken token)
  {
    var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters
    {
      All = true,
      Filters = new Dictionary<string, IDictionary<string, bool>>
      {
        ["name"] = new Dictionary<string, bool>
        {
          [name] = true
        }
      }
    }, token).ConfigureAwait(false) ?? throw new InvalidOperationException($"Could not find container '{name}'");

    return containers.FirstOrDefault()?.ID ?? string.Empty;
  }
}
