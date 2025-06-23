using Docker.DotNet.Models;

namespace DevantlerTech.ContainerEngineProvisioner.Podman.Tests.PodmanProvisionerTests;

/// <summary>
/// Tests for <see cref="PodmanProvisioner"/>.
/// </summary>
public class CreateDirectoryInContainerAsyncTests
{
  readonly PodmanProvisioner _podmanProvisioner = new();

  /// <summary>
  /// Tests the <see cref="PodmanProvisioner.CreateDirectoryInContainerAsync(string, string, bool, CancellationToken)"/> creates a directory in a container.
  /// </summary>
  /// <returns></returns>
  [SkippableFact]
  public async Task CreateDirectoryInContainerAsync_ValidParameters_CreatesDirectory()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    await _podmanProvisioner.Client.Images.CreateImageAsync(
      new ImagesCreateParameters
      {
        FromImage = "alpine",
        Tag = "latest",
      },
      null,
      new Progress<JSONMessage>()).ConfigureAwait(false);
    var createContainerResponse = await _podmanProvisioner.Client.Containers.CreateContainerAsync(new CreateContainerParameters
    {
      Image = "alpine:latest",
      Cmd = ["sleep", "inf"],
      Name = "create_directory_test_podman"
    }).ConfigureAwait(false);
    _ = await _podmanProvisioner.Client.Containers.StartContainerAsync(
      createContainerResponse.ID,
      new ContainerStartParameters()
    ).ConfigureAwait(false);
    string containerId = createContainerResponse.ID;
    string path = "/etc/new_directory";
    bool recursive = true;

    // Sleep 5 sec
    await Task.Delay(5000).ConfigureAwait(false);

    // Act
    await _podmanProvisioner.CreateDirectoryInContainerAsync(containerId, path, recursive).ConfigureAwait(false);
    var execCreateResponse = await _podmanProvisioner.Client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
    {
      AttachStdout = true,
      AttachStderr = true,
      Cmd = ["sh", "-c", $"if [ -d \"{path}\" ]; then echo \"Directory exists\"; else echo \"Directory does not exist\"; fi"]
    }).ConfigureAwait(false);

    // Assert
    using var execStream = await _podmanProvisioner.Client.Exec.StartAndAttachContainerExecAsync(execCreateResponse.ID, false).ConfigureAwait(false);
    var (stdout, _) = await execStream.ReadOutputToEndAsync(default).ConfigureAwait(false);
    string output = stdout;
    Assert.Equal("Directory exists", output.Trim());

    // Cleanup
    await _podmanProvisioner.Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
      Force = true
    }).ConfigureAwait(false);
  }
}
