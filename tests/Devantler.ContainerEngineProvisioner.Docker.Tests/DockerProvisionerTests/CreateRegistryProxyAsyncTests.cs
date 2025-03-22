using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Devantler.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

/// <summary>
/// Unit tests for <see cref="DockerProvisioner.CreateRegistryProxyAsync(string, int, ReadOnlyCollection{Uri}, CancellationToken)"/> and <see cref="DockerProvisioner.DeleteRegistryAsync(string, CancellationToken)"/>.
/// </summary>
public class CreateRegistryProxyAsyncTests
{
  readonly DockerProvisioner _provisioner = new();

  /// <summary>
  /// Tests whether the registry is created when it does not exist.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task CreateRegistryProxyAsync_RegistryDoesNotExist_CreatesRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if ((RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
    {
      return;
    }

    // Arrange
    string registryName = "docker-registry-proxy";
    int port = 6999;
    var cancellationToken = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryProxyAsync(registryName, port, new ReadOnlyCollection<Uri>([new("https://registry-1.docker.io"), new("https://ghcr.io")]), cancellationToken: cancellationToken);

    // Assert
    bool registryExists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken: cancellationToken);
    Assert.True(registryExists);

    // Cleanup
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Tests whether the registry is not created when it already exists.
  /// </summary>
  [Fact]
  public async Task CreateRegistryProxyAsync_RegistryExists_DoesNotCreateRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    // Arrange
    string registryName = "docker-registry-proxy";
    int port = 6999;
    var cancellationToken = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryProxyAsync(registryName, port, new ReadOnlyCollection<Uri>([new("https://registry-1.docker.io"), new("https://ghcr.io")]), cancellationToken: cancellationToken);
    await _provisioner.CreateRegistryProxyAsync(registryName, port, new ReadOnlyCollection<Uri>([new("https://registry-1.docker.io"), new("https://ghcr.io")]), cancellationToken: cancellationToken);

    // Assert
    bool registry1Exists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken);
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken);
    Assert.True(registry1Exists);
    bool registry2Exists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken);
    Assert.False(registry2Exists);
  }
}
