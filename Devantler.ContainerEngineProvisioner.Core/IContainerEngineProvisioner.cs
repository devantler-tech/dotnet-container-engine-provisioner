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
  /// Creates a registry in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="port"></param>
  /// <param name="proxyUrl"></param>
  /// <param name="cancellationToken"></param>
  Task CreateRegistryAsync(string name, int port, Uri? proxyUrl = default, CancellationToken cancellationToken = default);

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
}
