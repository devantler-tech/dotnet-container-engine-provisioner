using Docker.DotNet.Models;

namespace DevantlerTech.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

/// <summary>
/// Tests for <see cref="DockerProvisioner"/>.
/// </summary>
public class CreateDirectoryInContainerAsyncTests
{
  readonly DockerProvisioner _dockerProvisioner = new();

  /// <summary>
  /// Tests the <see cref="DockerProvisioner.CreateDirectoryInContainerAsync(string, string, bool, CancellationToken)"/> creates a directory in a container.
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
    await _dockerProvisioner.Client.Images.CreateImageAsync(
      new ImagesCreateParameters
      {
        FromImage = "alpine",
        Tag = "latest",
      },
      null,
      new Progress<JSONMessage>()).ConfigureAwait(false);
    var createContainerResponse = await _dockerProvisioner.Client.Containers.CreateContainerAsync(new CreateContainerParameters
    {
      Image = "alpine:latest",
      Cmd = ["sleep", "inf"],
      Name = "create_directory_test_docker"
    }).ConfigureAwait(false);
    _ = await _dockerProvisioner.Client.Containers.StartContainerAsync(
      createContainerResponse.ID,
      new ContainerStartParameters()
    ).ConfigureAwait(false);
    string containerId = createContainerResponse.ID;
    string path = "/etc/new_directory";
    bool recursive = true;

    // Sleep 5 sec
    await Task.Delay(5000).ConfigureAwait(false);

    // Act
    await _dockerProvisioner.CreateDirectoryInContainerAsync(containerId, path, recursive).ConfigureAwait(false);
    var execCreateResponse = await _dockerProvisioner.Client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
    {
      AttachStdout = true,
      AttachStderr = true,
      Cmd = ["sh", "-c", $"if [ -d \"{path}\" ]; then echo \"Directory exists\"; else echo \"Directory does not exist\"; fi"]
    }).ConfigureAwait(false);

    // Assert
    using var execStream = await _dockerProvisioner.Client.Exec.StartAndAttachContainerExecAsync(execCreateResponse.ID, false).ConfigureAwait(false);
    var (stdout, _) = await execStream.ReadOutputToEndAsync(default).ConfigureAwait(false);
    string output = stdout;
    Assert.Equal("Directory exists", output.Trim());

    // Cleanup
    await _dockerProvisioner.Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
      Force = true
    }).ConfigureAwait(false);
  }
}
