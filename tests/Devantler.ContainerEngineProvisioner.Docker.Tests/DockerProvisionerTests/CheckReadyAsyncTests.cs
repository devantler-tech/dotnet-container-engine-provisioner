using System.Runtime.InteropServices;

namespace Devantler.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

/// <summary>
/// Unit tests for <see cref="DockerProvisioner.CheckReadyAsync(CancellationToken)"/>.
/// </summary>
public class CheckReadyAsyncTests
{
  readonly DockerProvisioner _provisioner = new();

  /// <summary>
  /// Tests whether the the boolean value returned by the method is true when the Docker engine is ready.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task CheckReadyAsync_ReturnsTrue_WhenDockerIsReady()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    // Act
    bool containerEngineIsReady = await _provisioner.CheckReadyAsync(CancellationToken.None);

    // Assert
    Assert.True(containerEngineIsReady);
  }
}
