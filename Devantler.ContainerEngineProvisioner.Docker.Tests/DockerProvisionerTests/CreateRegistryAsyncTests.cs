namespace Devantler.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

/// <summary>
/// Unit tests for <see cref="DockerProvisioner.CreateRegistryAsync(string, int, CancellationToken, Uri?)"/> and <see cref="DockerProvisioner.DeleteRegistryAsync(string, CancellationToken)"/>.
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
    // Arrange
    string registryName = "new_registry";
    int port = 5999;
    var token = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, token);

    // Assert
    bool registryExists = await _provisioner.CheckContainerExistsAsync(registryName, token);
    Assert.True(registryExists);

    // Cleanup
    await _provisioner.DeleteRegistryAsync(registryName, token);
  }

  /// <summary>
  /// Tests whether the registry is not created when it already exists.
  /// </summary>
  [Fact]
  public async Task CreateRegistryAsync_RegistryExists_DoesNotCreateRegistry()
  {
    // Arrange
    string registryName = "new_registry";
    int port = 5999;
    var token = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, token);
    await _provisioner.CreateRegistryAsync(registryName, port, token);

    // Assert
    bool registry1Exists = await _provisioner.CheckContainerExistsAsync(registryName, token);
    await _provisioner.DeleteRegistryAsync(registryName, token);
    Assert.True(registry1Exists);
    bool registry2Exists = await _provisioner.CheckContainerExistsAsync(registryName, token);
    Assert.False(registry2Exists);
  }

  /// <summary>
  /// Tests whether a pull-through registry is created when a proxy URL is provided.
  /// </summary>
  [Fact]
  public async Task CreateRegistryAsync_ProxyUrlProvided_CreatesPullThroughRegistry()
  {
    // Arrange
    string registryName = "new_registry";
    int port = 5999;
    var token = CancellationToken.None;
    Uri proxyUrl = new("http://proxy:8080");

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, token, proxyUrl);

    // Assert
    bool registryExists = await _provisioner.CheckContainerExistsAsync(registryName, token);
    Assert.True(registryExists);

    // Cleanup
    await _provisioner.DeleteRegistryAsync(registryName, token);
  }
}
