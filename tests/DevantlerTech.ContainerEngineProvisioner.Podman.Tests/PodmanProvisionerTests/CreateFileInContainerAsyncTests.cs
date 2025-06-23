using Docker.DotNet.Models;

namespace DevantlerTech.ContainerEngineProvisioner.Podman.Tests.PodmanProvisionerTests;

/// <summary>
/// Tests for <see cref="PodmanProvisioner"/>.
/// </summary>
public class CreateFileInContainerAsyncTests
{
  readonly PodmanProvisioner _podmanProvisioner = new();

  /// <summary>
  /// Tests the <see cref="PodmanProvisioner.CreateDirectoryInContainerAsync(string, string, bool, CancellationToken)"/> creates a directory in a container.
  /// </summary>
  /// <returns></returns>
  [SkippableFact]
  public async Task CreateFileInContainerAsync_ValidParameters_CreatesFile()
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
      Name = "create_file_test_podman"
    }).ConfigureAwait(false);
    _ = await _podmanProvisioner.Client.Containers.StartContainerAsync(
      createContainerResponse.ID,
      new ContainerStartParameters()
    ).ConfigureAwait(false);
    string containerId = createContainerResponse.ID;
    string fileContent = "Hello, World!";
    string filePath = "/etc/hello.txt";

    // Sleep 5 sec
    await Task.Delay(5000).ConfigureAwait(false);

    // Act
    await _podmanProvisioner.CreateFileInContainerAsync(containerId, filePath, fileContent).ConfigureAwait(false);
    var execCreateResponse = await _podmanProvisioner.Client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
    {
      AttachStdout = true,
      AttachStderr = true,
      Cmd = ["sh", "-c", $"if [ -f \"{filePath}\" ]; then echo \"File exists\"; else echo \"File does not exist\"; fi"]
    }).ConfigureAwait(false);
    using var execStream = await _podmanProvisioner.Client.Exec.StartAndAttachContainerExecAsync(execCreateResponse.ID, false).ConfigureAwait(false);

    // Assert
    var output = await execStream.ReadOutputToEndAsync(CancellationToken.None).ConfigureAwait(false);
    string stdout = output.stdout;
    Assert.Equal("File exists", stdout.Trim());

    // Cleanup
    await _podmanProvisioner.Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
      Force = true
    }).ConfigureAwait(false);
  }
}
