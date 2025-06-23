using System.Collections.ObjectModel;

namespace DevantlerTech.ContainerEngineProvisioner.Podman.Tests.PodmanProvisionerTests;

/// <summary>
/// Unit tests for <see cref="PodmanProvisioner.CreateRegistryProxyAsync(string, int, ReadOnlyCollection{Uri}, CancellationToken)"/> and <see cref="PodmanProvisioner.DeleteRegistryAsync(string, CancellationToken)"/>.
/// </summary>
public class CreateRegistryProxyAsyncTests
{
  readonly PodmanProvisioner _podmanProvisioner = new();

  /// <summary>
  /// Tests whether the registry is created when it does not exist.
  /// </summary>
  /// <returns></returns>
  [SkippableFact]
  public async Task CreateRegistryProxyAsync_RegistryDoesNotExist_CreatesRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    string registryName = "docker-registry-proxy_podman";
    int port = 6998;
    var cancellationToken = CancellationToken.None;

    // Act
    await _podmanProvisioner.CreateRegistryProxyAsync(registryName, port, new ReadOnlyCollection<Uri>([new("https://registry-1.docker.io"), new("https://ghcr.io")]), cancellationToken: cancellationToken).ConfigureAwait(false);

    // Assert
    bool registryExists = await _podmanProvisioner.CheckContainerExistsAsync(registryName, cancellationToken: cancellationToken).ConfigureAwait(false);
    Assert.True(registryExists);

    // Cleanup
    await _podmanProvisioner.DeleteRegistryAsync(registryName, cancellationToken: cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Tests whether the registry is not created when it already exists.
  /// </summary>
  [SkippableFact]
  public async Task CreateRegistryProxyAsync_RegistryExists_DoesNotCreateRegistry()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    string registryName = "docker-registry-proxy_podman";
    int port = 6998;
    var cancellationToken = CancellationToken.None;

    // Act
    await _podmanProvisioner.CreateRegistryProxyAsync(registryName, port, new ReadOnlyCollection<Uri>([new("https://registry-1.docker.io"), new("https://ghcr.io")]), cancellationToken: cancellationToken).ConfigureAwait(false);
    await _podmanProvisioner.CreateRegistryProxyAsync(registryName, port, new ReadOnlyCollection<Uri>([new("https://registry-1.docker.io"), new("https://ghcr.io")]), cancellationToken: cancellationToken).ConfigureAwait(false);

    // Assert
    bool registry1Exists = await _podmanProvisioner.CheckContainerExistsAsync(registryName, cancellationToken).ConfigureAwait(false);
    await _podmanProvisioner.DeleteRegistryAsync(registryName, cancellationToken).ConfigureAwait(false);
    Assert.True(registry1Exists);
    bool registry2Exists = await _podmanProvisioner.CheckContainerExistsAsync(registryName, cancellationToken).ConfigureAwait(false);
    Assert.False(registry2Exists);
  }
}
