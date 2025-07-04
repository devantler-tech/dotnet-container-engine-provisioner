using Docker.DotNet.Models;

namespace DevantlerTech.ContainerEngineProvisioner.Podman.Tests.PodmanProvisionerTests;

/// <summary>
/// Unit tests for <see cref="PodmanProvisioner.ConnectContainerToNetworkByNameAsync(string, string, CancellationToken)"/>.
/// </summary>
public class ConnectContainerToNetworkByNameAsyncTests
{
  readonly PodmanProvisioner _podmanProvisioner = new();

  /// <summary>
  /// Tests the <see cref="PodmanProvisioner.ConnectContainerToNetworkByNameAsync(string, string, CancellationToken)"/> method.
  /// </summary>
  /// <returns></returns>
  [SkippableFact]
  public async Task ConnectContainerToNetworkByNameAsync_WhenCalled_ConnectsContainerToNetwork()
  {
    //TODO: Support MacOS and Windows when GitHub Actions runners supports dind.
    Skip.If(
      (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) && Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
      "Skipping test on Windows and MacOS in GitHub Actions."
    );

    // Arrange
    string containerName = "connect_container_to_network_by_name_test_podman";
    string networkName = "test_network_by_name_podman";
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
      Name = containerName
    }).ConfigureAwait(false);
    _ = await _podmanProvisioner.Client.Containers.StartContainerAsync(
      createContainerResponse.ID,
      new ContainerStartParameters()
    ).ConfigureAwait(false);
    var createNetworkResponse = await _podmanProvisioner.Client.Networks.CreateNetworkAsync(new NetworksCreateParameters
    {
      Name = networkName,
      Driver = "bridge"
    }).ConfigureAwait(false);

    // Act
    await _podmanProvisioner.ConnectContainerToNetworkByNameAsync(containerName, networkName).ConfigureAwait(false);

    // Assert
    var network = await _podmanProvisioner.Client.Networks.InspectNetworkAsync(createNetworkResponse.ID).ConfigureAwait(false);
    var container = network.Containers.FirstOrDefault(c => c.Value.Name == containerName);
    Assert.NotNull(network);
    Assert.Equal(networkName, network.Name);
    Assert.Equal(createNetworkResponse.ID, network.ID);
    Assert.Equal(containerName, container.Value.Name);

    // Cleanup
    _ = await _podmanProvisioner.Client.Containers.StopContainerAsync(createContainerResponse.ID, new ContainerStopParameters()).ConfigureAwait(false);
    await _podmanProvisioner.Client.Containers.RemoveContainerAsync(createContainerResponse.ID, new ContainerRemoveParameters()).ConfigureAwait(false);
    await _podmanProvisioner.Client.Networks.DeleteNetworkAsync(createNetworkResponse.ID).ConfigureAwait(false);
  }
}
