using System.Runtime.InteropServices;

namespace Devantler.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

/// <summary>
/// Unit tests for <see cref="DockerProvisioner.CreateRegistryAsync(string, int, Uri?, CancellationToken)"/> and <see cref="DockerProvisioner.DeleteRegistryAsync(string, CancellationToken)"/>.
/// </summary>
public class CreateRegistryAsyncTests
{
  readonly DockerProvisioner _provisioner = new();

  /// <summary>
  /// Tests whether the registry is created when it does not exist.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task CreateRegistryAsync_RegistryDoesNotExist_CreatesRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    // Arrange
    string registryName = "new_registry";
    int port = 5999;
    var cancellationToken = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, cancellationToken: cancellationToken);

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
  public async Task CreateRegistryAsync_RegistryExists_DoesNotCreateRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    // Arrange
    string registryName = "new_registry";
    int port = 5999;
    var cancellationToken = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, cancellationToken: cancellationToken);
    await _provisioner.CreateRegistryAsync(registryName, port, cancellationToken: cancellationToken);

    // Assert
    bool registry1Exists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken);
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken);
    Assert.True(registry1Exists);
    bool registry2Exists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken);
    Assert.False(registry2Exists);
  }

  /// <summary>
  /// Tests whether a pull-through registry is created when a proxy URL is provided.
  /// </summary>
  [Fact]
  public async Task CreateRegistryAsync_ProxyUrlProvided_CreatesPullThroughRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    // Arrange
    string registryName = "new_registry";
    int port = 5999;
    var cancellationToken = CancellationToken.None;
    Uri proxyUrl = new("http://proxy:8080");

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, proxyUrl, cancellationToken);

    // Assert
    bool registryExists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken);
    Assert.True(registryExists);

    // Cleanup
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken);
  }
}
