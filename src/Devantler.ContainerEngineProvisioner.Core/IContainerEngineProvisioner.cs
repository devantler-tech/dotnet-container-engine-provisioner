using System.Collections.ObjectModel;

namespace Devantler.ContainerEngineProvisioner.Core;

/// <summary>
/// A container engine provisioner capable of modifying resources in a container engine.
/// </summary>
public interface IContainerEngineProvisioner
{
  /// <summary>
  /// Checks if the container engine is ready.
  /// </summary>
  /// <param name="cancellationToken"></param>
  Task<bool> CheckReadyAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a 'docker.io/registry' in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="port"></param>
  /// <param name="proxyUrl"></param>
  /// <param name="cancellationToken"></param>
  Task CreateRegistryAsync(string name, int port, Uri? proxyUrl = default, CancellationToken cancellationToken = default);

  /// <summary>
  /// Pulls a docker image from a remote registry.
  /// </summary>
  /// <param name="imageName"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task PullImageAsync(string imageName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a docker image exists in the local registry.
  /// </summary>
  /// <param name="imageName"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<bool> CheckImageExistsAsync(string imageName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a 'rpardini/docker-registry-proxy' in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="port"></param>
  /// <param name="proxyUrls"></param>
  /// <param name="cancellationToken"></param>
  Task CreateRegistryProxyAsync(string name, int port, ReadOnlyCollection<Uri> proxyUrls, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes a registry in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="cancellationToken"></param>
  Task DeleteRegistryAsync(string name, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the container ID of a container in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="cancellationToken"></param>
  Task<string> GetContainerIdAsync(string name, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks that the container exists in the container engine.
  /// </summary>
  Task<bool> CheckContainerExistsAsync(string name, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a directory in a container.
  /// </summary>
  Task CreateDirectoryInContainerAsync(string containerId, string path, bool recursive, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a file in a container.
  /// </summary>
  Task CreateFileInContainerAsync(string containerId, string path, string content, CancellationToken cancellationToken = default);

  /// <summary>
  /// Connects a container to a network by name.
  /// </summary>
  Task ConnectContainerToNetworkByNameAsync(string containerName, string networkName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Connects a container to a network by ID.
  /// </summary>
  /// <param name="containerId"></param>
  /// <param name="networkId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task ConnectContainerToNetworkByIdAsync(string containerId, string networkId, CancellationToken cancellationToken = default);
}
