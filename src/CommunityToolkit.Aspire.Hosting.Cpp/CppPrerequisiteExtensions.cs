using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

internal enum CppCompilerTool
{
    Msvc,
    Clang,
    Gcc
}

internal enum CppPackageManagerTool
{
    Vcpkg,
    Conan
}

/// <summary>
/// Stores user-facing prerequisite validation messages for C++ tooling.
/// </summary>
internal static class CppPrerequisiteMessages
{
    internal const string CMakeMissing = "cmake is not installed or not available on PATH. Install CMake and ensure the cmake command is available.";
    internal const string MsvcMissing = "MSVC build tools are not installed or not available. Install the Visual Studio C++ toolchain and run Aspire from a Developer Command Prompt or activated build environment.";
    internal const string ClangMissing = "clang++ is not installed or not available on PATH. Install LLVM/Clang and ensure clang++ is available.";
    internal const string GccMissing = "g++ is not installed or not available on PATH. Install GCC/G++ and ensure g++ is available.";
    internal const string VcpkgMissing = "vcpkg is not installed or not available on PATH. Install vcpkg and ensure the vcpkg command is available.";
    internal const string ConanMissing = "conan is not installed or not available on PATH. Install Conan and ensure the conan command is available.";
}

/// <summary>
/// Provides internal helpers for attaching prerequisite guidance to C++ resources.
/// </summary>
internal static class CppPrerequisiteExtensions
{
    internal static IResourceBuilder<T> WithRequiredCppCommand<T>(
        this IResourceBuilder<T> builder,
        string command,
        string helpLink) where T : IResource
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentException.ThrowIfNullOrWhiteSpace(command, nameof(command));
        ArgumentException.ThrowIfNullOrWhiteSpace(helpLink, nameof(helpLink));

#pragma warning disable ASPIRECOMMAND001
        return builder.WithRequiredCommand(command, helpLink);
#pragma warning restore ASPIRECOMMAND001
    }

    internal static IResourceBuilder<T> WithCMakePrerequisite<T>(this IResourceBuilder<T> builder)
        where T : IResource
        => builder.WithValidatedCppCommand("cmake", CppToolInstallLinks.CMake, CppPrerequisiteMessages.CMakeMissing);

    internal static IResourceBuilder<T> WithCompilerPrerequisite<T>(
        this IResourceBuilder<T> builder,
        CppCompilerTool compiler) where T : IResource
        => compiler switch
        {
            CppCompilerTool.Msvc => builder.WithValidatedCppCommand("cl", CppToolInstallLinks.Msvc, CppPrerequisiteMessages.MsvcMissing),
            CppCompilerTool.Clang => builder.WithValidatedCppCommand("clang++", CppToolInstallLinks.Clang, CppPrerequisiteMessages.ClangMissing),
            CppCompilerTool.Gcc => builder.WithValidatedCppCommand("g++", CppToolInstallLinks.Gcc, CppPrerequisiteMessages.GccMissing),
            _ => throw new ArgumentOutOfRangeException(nameof(compiler), compiler, null)
        };

    internal static IResourceBuilder<T> WithPackageManagerPrerequisite<T>(
        this IResourceBuilder<T> builder,
        CppPackageManagerTool packageManager) where T : IResource
        => packageManager switch
        {
            CppPackageManagerTool.Vcpkg => builder.WithValidatedCppCommand("vcpkg", CppToolInstallLinks.Vcpkg, CppPrerequisiteMessages.VcpkgMissing),
            CppPackageManagerTool.Conan => builder.WithValidatedCppCommand("conan", CppToolInstallLinks.Conan, CppPrerequisiteMessages.ConanMissing),
            _ => throw new ArgumentOutOfRangeException(nameof(packageManager), packageManager, null)
        };

#pragma warning disable ASPIRECOMMAND001
    internal static ValueTask<RequiredCommandValidationResult> ValidateCommandAvailableAsync(
        string resolvedPath,
        string? pathValue,
        Func<string, bool> fileExists,
        string failureMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resolvedPath, nameof(resolvedPath));
        ArgumentNullException.ThrowIfNull(fileExists, nameof(fileExists));
        ArgumentException.ThrowIfNullOrWhiteSpace(failureMessage, nameof(failureMessage));

#pragma warning disable ASPIRECOMMAND001
        return ValueTask.FromResult(
            IsCommandAvailable(resolvedPath, pathValue, fileExists)
                ? RequiredCommandValidationResult.Success()
                : RequiredCommandValidationResult.Failure(failureMessage));
#pragma warning restore ASPIRECOMMAND001
    }
#pragma warning restore ASPIRECOMMAND001

    internal static bool IsCommandAvailable(string resolvedPath, string? pathValue, Func<string, bool> fileExists)
    {
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return false;
        }

        if (fileExists(resolvedPath))
        {
            return true;
        }

        return TryResolveCommandFromPath(resolvedPath, pathValue, fileExists) is not null;
    }

    internal static string? TryResolveCommandFromPath(string commandName, string? pathValue, Func<string, bool> fileExists)
    {
        string candidateName = Path.GetFileName(commandName);
        if (string.IsNullOrWhiteSpace(candidateName) || string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        List<string> candidates = [candidateName];

        if (OperatingSystem.IsWindows())
        {
            string pathExt = Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT";
            foreach (string extension in pathExt.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!candidateName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add($"{candidateName}{extension}");
                }
            }
        }

        foreach (string directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (string candidate in candidates)
            {
                string fullPath = Path.Combine(directory, candidate);
                if (fileExists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    private static IResourceBuilder<T> WithValidatedCppCommand<T>(
        this IResourceBuilder<T> builder,
        string command,
        string helpLink,
        string failureMessage) where T : IResource
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentException.ThrowIfNullOrWhiteSpace(command, nameof(command));
        ArgumentException.ThrowIfNullOrWhiteSpace(helpLink, nameof(helpLink));
        ArgumentException.ThrowIfNullOrWhiteSpace(failureMessage, nameof(failureMessage));

#pragma warning disable ASPIRECOMMAND001
        return builder.WithRequiredCommand(
            command,
            context => ValidateCommandAvailableAsync(
                    context.ResolvedPath,
                    Environment.GetEnvironmentVariable("PATH"),
                    File.Exists,
                    failureMessage)
                .AsTask(),
            helpLink);
#pragma warning restore ASPIRECOMMAND001
    }
}