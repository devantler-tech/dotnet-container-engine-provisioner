using Docker.DotNet.Models;

namespace Devantler.ContainerEngineProvisioner.Docker.Tests.DockerProvisionerTests;

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
  [Fact]
  public async Task CreateDirectoryInContainerAsync_ValidParameters_CreatesDirectory()
  {
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
      Name = "create_directory_test"
    });
    _ = await _dockerProvisioner.Client.Containers.StartContainerAsync(
      createContainerResponse.ID,
      new ContainerStartParameters()
    );
    string containerId = createContainerResponse.ID;
    string path = "/etc/new_directory";
    bool recursive = true;

    // Sleep 5 sec
    await Task.Delay(5000);

    // Act
    async Task task() => await _dockerProvisioner.CreateDirectoryInContainerAsync(containerId, path, recursive);

    // Assert
    Assert.Null(await Record.ExceptionAsync(task));
    // TODO: Verify the directory is created in the container

    // Cleanup
    await _dockerProvisioner.Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
      Force = true
    });
    _ = await _dockerProvisioner.Client.Images.DeleteImageAsync(
      "alpine:latest",
      new ImageDeleteParameters()
    );
  }
}
