namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a C++ application executable.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="executablePath">The executable path to run.</param>
/// <param name="workingDirectory">The working directory to use for the executable.</param>
public class CppAppExecutableResource(string name, string executablePath, string workingDirectory)
    : ExecutableResource(name, executablePath, workingDirectory), IResourceWithServiceDiscovery, IResourceWithWaitSupport
{
}