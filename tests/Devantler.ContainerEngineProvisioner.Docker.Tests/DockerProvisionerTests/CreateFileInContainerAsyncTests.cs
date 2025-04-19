using System.Runtime.InteropServices;
using Docker.DotNet.Models;

namespace Devantler.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

/// <summary>
/// Tests for <see cref="DockerProvisioner"/>.
/// </summary>
public class CreateFileInContainerAsyncTests
{
  readonly DockerProvisioner _dockerProvisioner = new();

  /// <summary>
  /// Tests the <see cref="DockerProvisioner.CreateDirectoryInContainerAsync(string, string, bool, CancellationToken)"/> creates a directory in a container.
  /// </summary>
  /// <returns></returns>
  [Fact]
  public async Task CreateFileInContainerAsync_ValidParameters_CreatesFile()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    // Arrange
    await _dockerProvisioner.Client.Images.CreateImageAsync(
      new ImagesCreateParameters
      {
        FromImage = "alpine",
        Tag = "latest",
      },
      null,
      new Progress<JSONMessage>());
    var createContainerResponse = await _dockerProvisioner.Client.Containers.CreateContainerAsync(new CreateContainerParameters
    {
      Image = "alpine:latest",
      Cmd = ["sleep", "inf"],
      Name = "create_file_test"
    });
    _ = await _dockerProvisioner.Client.Containers.StartContainerAsync(
      createContainerResponse.ID,
      new ContainerStartParameters()
    );
    string containerId = createContainerResponse.ID;
    string fileContent = "Hello, World!";
    string filePath = "/etc/hello.txt";

    // Sleep 5 sec
    await Task.Delay(5000);

    // Act
    async Task task() => await _dockerProvisioner.CreateFileInContainerAsync(containerId, filePath, fileContent).ConfigureAwait(false);
    // Check if the file exists
    var execCreateResponse = await _dockerProvisioner.Client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
    {
      AttachStdout = true,
      AttachStderr = true,
      Cmd = ["sh", "-c", $"if [ -f \"{filePath}\" ]; then echo \"File exists\"; else echo \"File does not exist\"; fi"]
    });
    using var execStream = await _dockerProvisioner.Client.Exec.StartAndAttachContainerExecAsync(execCreateResponse.ID, false);
    var output = await execStream.ReadOutputToEndAsync(CancellationToken.None);
    string stdout = output.stdout;

    // Assert
    Assert.Null(await Record.ExceptionAsync(task));
    Assert.Equal("File exists", stdout.Trim());

    // Cleanup
    await _dockerProvisioner.Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
      Force = true
    });
  }
}
