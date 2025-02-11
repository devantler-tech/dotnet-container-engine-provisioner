namespace Devantler.ContainerEngineProvisioner.Core;

/// <summary>
/// An exception thrown by a container engine provisioner.
/// </summary>
[Serializable]
public class ContainerEngineProvisionerException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ContainerEngineProvisionerException"/> class.
  /// </summary>
  public ContainerEngineProvisionerException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ContainerEngineProvisionerException"/> class with a specified error message.
  /// </summary>
  /// <param name="message"></param>
  public ContainerEngineProvisionerException(string? message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ContainerEngineProvisionerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message"></param>
  /// <param name="innerException"></param>
  public ContainerEngineProvisionerException(string? message, Exception? innerException) : base(message, innerException)
  {
  }
}
