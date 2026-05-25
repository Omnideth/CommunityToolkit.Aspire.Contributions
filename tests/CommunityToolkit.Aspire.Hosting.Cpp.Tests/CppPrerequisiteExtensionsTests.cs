using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace CommunityToolkit.Aspire.Hosting.Cpp.Tests;

public class CppPrerequisiteExtensionsTests
{
    [Fact]
    public void WithRequiredCppCommandAddsRequiredCommandAnnotation()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        builder.AddCppExecutable("cpp-app", ".", "cpp-app")
            .WithRequiredCppCommand("cmake", CppToolInstallLinks.CMake);

        using DistributedApplication app = builder.Build();

        DistributedApplicationModel appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        CppAppExecutableResource resource = Assert.Single(appModel.Resources.OfType<CppAppExecutableResource>());

#pragma warning disable ASPIRECOMMAND001
        RequiredCommandAnnotation annotation = resource.Annotations.OfType<RequiredCommandAnnotation>()
            .Single(a => a.Command == "cmake");
#pragma warning restore ASPIRECOMMAND001

        Assert.Equal(CppToolInstallLinks.CMake, annotation.HelpLink);
        Assert.Null(annotation.ValidationCallback);
    }

    [Fact]
    public void WithCMakePrerequisiteAddsValidatedRequiredCommandAnnotation()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        builder.AddCppExecutable("cpp-app", ".", "cpp-app")
            .WithCMakePrerequisite();

        using DistributedApplication app = builder.Build();

        DistributedApplicationModel appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        CppAppExecutableResource resource = Assert.Single(appModel.Resources.OfType<CppAppExecutableResource>());

#pragma warning disable ASPIRECOMMAND001
        RequiredCommandAnnotation annotation = resource.Annotations.OfType<RequiredCommandAnnotation>()
            .Single(a => a.Command == "cmake");
#pragma warning restore ASPIRECOMMAND001

        Assert.Equal(CppToolInstallLinks.CMake, annotation.HelpLink);
        Assert.NotNull(annotation.ValidationCallback);
    }

    [Fact]
    public async Task WithCMakePrerequisiteValidationFailsWhenCMakeCannotBeResolved()
    {
        var result = await CppPrerequisiteExtensions.ValidateCommandAvailableAsync(
            resolvedPath: "cmake",
            pathValue: null,
            fileExists: _ => false,
            failureMessage: CppPrerequisiteMessages.CMakeMissing);

        Assert.False(result.IsValid);
        Assert.Equal(CppPrerequisiteMessages.CMakeMissing, result.ValidationMessage);
    }

    [Fact]
    public async Task WithCMakePrerequisiteValidationSucceedsWhenCMakeCanBeResolved()
    {
        var result = await CppPrerequisiteExtensions.ValidateCommandAvailableAsync(
            resolvedPath: "cmake",
            pathValue: @"C:\tools",
            fileExists: path => string.Equals(path, @"C:\tools\cmake.exe", StringComparison.OrdinalIgnoreCase),
            failureMessage: CppPrerequisiteMessages.CMakeMissing);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void WithCompilerPrerequisiteForClangAddsClangRequiredCommandAnnotation()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        builder.AddCppExecutable("cpp-app", ".", "cpp-app")
            .WithCompilerPrerequisite(CppCompilerTool.Clang);

        using DistributedApplication app = builder.Build();

        DistributedApplicationModel appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        CppAppExecutableResource resource = Assert.Single(appModel.Resources.OfType<CppAppExecutableResource>());

#pragma warning disable ASPIRECOMMAND001
        RequiredCommandAnnotation annotation = resource.Annotations.OfType<RequiredCommandAnnotation>()
            .Single(a => a.Command == "clang++");
#pragma warning restore ASPIRECOMMAND001

        Assert.Equal(CppToolInstallLinks.Clang, annotation.HelpLink);
        Assert.NotNull(annotation.ValidationCallback);
    }

    [Fact]
    public void WithPackageManagerPrerequisiteForVcpkgAddsVcpkgRequiredCommandAnnotation()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        builder.AddCppExecutable("cpp-app", ".", "cpp-app")
            .WithPackageManagerPrerequisite(CppPackageManagerTool.Vcpkg);

        using DistributedApplication app = builder.Build();

        DistributedApplicationModel appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        CppAppExecutableResource resource = Assert.Single(appModel.Resources.OfType<CppAppExecutableResource>());

#pragma warning disable ASPIRECOMMAND001
        RequiredCommandAnnotation annotation = resource.Annotations.OfType<RequiredCommandAnnotation>()
            .Single(a => a.Command == "vcpkg");
#pragma warning restore ASPIRECOMMAND001

        Assert.Equal(CppToolInstallLinks.Vcpkg, annotation.HelpLink);
        Assert.NotNull(annotation.ValidationCallback);
    }

    [Fact]
    public void ToolInstallLinksContainAbsoluteUrls()
    {
        string[] links =
        [
            CppToolInstallLinks.CMake,
            CppToolInstallLinks.Msvc,
            CppToolInstallLinks.Clang,
            CppToolInstallLinks.Gcc,
            CppToolInstallLinks.Vcpkg,
            CppToolInstallLinks.Conan
        ];

        Assert.All(links, link => Assert.True(Uri.TryCreate(link, UriKind.Absolute, out _), $"Expected an absolute URI but got '{link}'."));
    }
}