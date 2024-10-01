namespace Devantler.ContainerEngineProvisioner.Core;

/// <summary>
/// A container engine provisioner capable of modifying resources in a container engine.
/// </summary>
public interface IContainerEngineProvisioner
{
  /// <summary>
  /// Checks if the container engine is ready.
  /// </summary>
  /// <param name="token"></param>
  Task<bool> CheckReadyAsync(CancellationToken token);

  /// <summary>
  /// Creates a registry in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="port"></param>
  /// <param name="token"></param>
  /// <param name="proxyUrl"></param>
  Task CreateRegistryAsync(string name, int port, CancellationToken token, Uri? proxyUrl = null);

  /// <summary>
  /// Deletes a registry in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="token"></param>
  Task DeleteRegistryAsync(string name, CancellationToken token);

  /// <summary>
  /// Gets the container ID of a container in the container engine.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="token"></param>
  Task<string> GetContainerIdAsync(string name, CancellationToken token);

  /// <summary>
  /// Checks that the container exists in the container engine.
  /// </summary>
  Task<bool> CheckContainerExistsAsync(string name, CancellationToken token);
}
