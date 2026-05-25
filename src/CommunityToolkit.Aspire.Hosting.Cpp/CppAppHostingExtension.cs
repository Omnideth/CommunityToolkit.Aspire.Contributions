using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding C++ applications to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class CppAppHostingExtension
{
    /// <summary>
    /// Adds a prebuilt C++ executable to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to add the resource to.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="workingDirectory">The working directory to use for the executable.</param>
    /// <param name="executablePath">The path to the executable, relative to the working directory unless rooted.</param>
    /// <param name="args">The optional arguments to pass when the executable starts.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    /// <remarks>
    /// This is the initial scaffolding entry point for the C++ integration. Build orchestration will be layered on top in later roadmap steps.
    /// </remarks>
    public static IResourceBuilder<CppAppExecutableResource> AddCppExecutable(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        string workingDirectory,
        string executablePath,
        string[]? args = null)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory, nameof(workingDirectory));
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath, nameof(executablePath));

        string resolvedWorkingDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, workingDirectory));
        string resolvedExecutablePath = Path.IsPathRooted(executablePath)
            ? executablePath
            : Path.GetFullPath(Path.Combine(resolvedWorkingDirectory, executablePath));

        CppAppExecutableResource resource = new(name, resolvedExecutablePath, resolvedWorkingDirectory);

        IResourceBuilder<CppAppExecutableResource> resourceBuilder = builder.AddResource(resource)
            .WithOtlpExporter();

        if (args is { Length: > 0 })
        {
            resourceBuilder.WithArgs(args);
        }

        return resourceBuilder;
    }
}