namespace DevantlerTech.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

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
  [SkippableFact]
  public async Task CreateRegistryAsync_RegistryDoesNotExist_CreatesRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    string registryName = "registry_docker";
    int port = 5999;
    var cancellationToken = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, cancellationToken: cancellationToken).ConfigureAwait(false);

    // Assert
    bool registryExists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken: cancellationToken).ConfigureAwait(false);
    Assert.True(registryExists);

    // Cleanup
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken: cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Tests whether the registry is not created when it already exists.
  /// </summary>
  [SkippableFact]
  public async Task CreateRegistryAsync_RegistryExists_DoesNotCreateRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    string registryName = "registry_docker";
    int port = 5999;
    var cancellationToken = CancellationToken.None;

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, cancellationToken: cancellationToken).ConfigureAwait(false);
    await _provisioner.CreateRegistryAsync(registryName, port, cancellationToken: cancellationToken).ConfigureAwait(false);

    // Assert
    bool registry1Exists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken).ConfigureAwait(false);
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken).ConfigureAwait(false);
    Assert.True(registry1Exists);
    bool registry2Exists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken).ConfigureAwait(false);
    Assert.False(registry2Exists);
  }

  /// <summary>
  /// Tests whether a pull-through registry is created when a proxy URL is provided.
  /// </summary>
  [SkippableFact]
  public async Task CreateRegistryAsync_ProxyUrlProvided_CreatesPullThroughRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    string registryName = "registry_docker";
    int port = 5999;
    var cancellationToken = CancellationToken.None;
    Uri proxyUrl = new("http://proxy:8080");

    // Act
    await _provisioner.CreateRegistryAsync(registryName, port, proxyUrl, cancellationToken).ConfigureAwait(false);

    // Assert
    bool registryExists = await _provisioner.CheckContainerExistsAsync(registryName, cancellationToken).ConfigureAwait(false);
    Assert.True(registryExists);

    // Cleanup
    await _provisioner.DeleteRegistryAsync(registryName, cancellationToken).ConfigureAwait(false);
  }
}
