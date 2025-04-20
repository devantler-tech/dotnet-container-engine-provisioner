using System.Collections.ObjectModel;
using Devantler.ContainerEngineProvisioner.Core;
using Devantler.ContainerEngineProvisioner.Docker;
using Docker.DotNet;

namespace Devantler.ContainerEngineProvisioner.Podman;

/// <summary>
/// A provisioner for Docker.
/// </summary>
public sealed class PodmanProvisioner : IContainerEngineProvisioner
{
  /// <summary>
  /// The provisioner
  /// </summary>
  DockerProvisioner _provisioner { get; }

  /// <summary>
  /// The Docker client
  /// </summary>
  public DockerClient Client => _provisioner.Client;

  /// <summary>
  /// Initializes a new instance of the <see cref="PodmanProvisioner"/> class.
  /// </summary>
  public PodmanProvisioner(string? dockerSocket = default)
  {
    string podmanSocket = !string.IsNullOrEmpty(dockerSocket) ? dockerSocket : File.Exists($"/run/user/{Environment.GetEnvironmentVariable("EUID")}/podman/podman.sock") ?
      $"unix:///run/user/${Environment.GetEnvironmentVariable("EUID")}/podman/podman.sock" : File.Exists($"/run/user/{Environment.GetEnvironmentVariable("UID")}/podman/podman.sock") ?
      $"unix:///run/user/${Environment.GetEnvironmentVariable("UID")}/podman/podman.sock" : File.Exists("/run/podman/podman.sock") ?
      "unix:///run/podman/podman.sock" : "unix:///var/run/docker.sock";
    _provisioner = new DockerProvisioner(podmanSocket);
  }

  /// <inheritdoc/>
  public async Task<bool> CheckContainerExistsAsync(string name, CancellationToken cancellationToken = default) =>
    await _provisioner.CheckContainerExistsAsync(name, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task<bool> CheckReadyAsync(CancellationToken cancellationToken = default) =>
    await _provisioner.CheckReadyAsync(cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task CreateDirectoryInContainerAsync(string containerId, string path, bool recursive, CancellationToken cancellationToken = default) =>
    await _provisioner.CreateDirectoryInContainerAsync(containerId, path, recursive, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task CreateFileInContainerAsync(string containerId, string path, string content, CancellationToken cancellationToken = default) =>
    await _provisioner.CreateFileInContainerAsync(containerId, path, content, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task CreateRegistryAsync(string name, int port, Uri? proxyUrl = default, CancellationToken cancellationToken = default) =>
    await _provisioner.CreateRegistryAsync(name, port, proxyUrl, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task CreateRegistryProxyAsync(string name, int port, ReadOnlyCollection<Uri> proxyUrls, CancellationToken cancellationToken = default) =>
    await _provisioner.CreateRegistryProxyAsync(name, port, proxyUrls, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task DeleteRegistryAsync(string name, CancellationToken cancellationToken = default) =>
    await _provisioner.DeleteRegistryAsync(name, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task<string> GetContainerIdAsync(string name, CancellationToken cancellationToken = default) =>
    await _provisioner.GetContainerIdAsync(name, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task ConnectContainerToNetworkByNameAsync(string containerName, string networkName, CancellationToken cancellationToken = default) =>
    await _provisioner.ConnectContainerToNetworkByIdAsync(containerName, networkName, cancellationToken).ConfigureAwait(false);

  /// <inheritdoc/>
  public async Task ConnectContainerToNetworkByIdAsync(string containerId, string networkId, CancellationToken cancellationToken = default) =>
    await _provisioner.ConnectContainerToNetworkByIdAsync(containerId, networkId, cancellationToken).ConfigureAwait(false);
}


