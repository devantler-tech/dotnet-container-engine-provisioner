namespace DevantlerTech.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

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
  [SkippableFact]
  public async Task CheckReadyAsync_ReturnsTrue_WhenDockerIsReady()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Act
    bool containerEngineIsReady = await _provisioner.CheckReadyAsync(CancellationToken.None).ConfigureAwait(false);

    // Assert
    Assert.True(containerEngineIsReady);
  }
}
