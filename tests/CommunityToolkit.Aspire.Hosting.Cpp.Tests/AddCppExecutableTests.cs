using Aspire.Hosting;
using CommunityToolkit.Aspire.Testing;

namespace CommunityToolkit.Aspire.Hosting.Cpp.Tests;

public class AddCppExecutableTests
{
    [Fact]
    public void AddCppExecutableBuilderShouldNotBeNull()
    {
        IDistributedApplicationBuilder builder = null!;

        Assert.Throws<ArgumentNullException>(() => builder.AddCppExecutable("cpp-app", ".", "bin/hello-world"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddCppExecutableNameShouldNotBeNullOrWhiteSpace(string? name)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        Assert.ThrowsAny<ArgumentException>(() => builder.AddCppExecutable(name!, ".", "bin/hello-world"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddCppExecutableWorkingDirectoryShouldNotBeNullOrWhiteSpace(string? workingDirectory)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        Assert.ThrowsAny<ArgumentException>(() => builder.AddCppExecutable("cpp-app", workingDirectory!, "bin/hello-world"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddCppExecutablePathShouldNotBeNullOrWhiteSpace(string? executablePath)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        Assert.ThrowsAny<ArgumentException>(() => builder.AddCppExecutable("cpp-app", ".", executablePath!));
    }

    [Fact]
    public async Task AddCppExecutableSetsExpectedResourceShape()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();

        builder.AddCppExecutable("cpp-app", "cpp-app", "bin/hello-world", ["--demo"]);

        using DistributedApplication app = builder.Build();

        DistributedApplicationModel appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        CppAppExecutableResource resource = Assert.Single(appModel.Resources.OfType<CppAppExecutableResource>());

        string expectedWorkingDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "cpp-app"));
        string expectedExecutablePath = Path.GetFullPath(Path.Combine(expectedWorkingDirectory, "bin/hello-world"));

        Assert.Equal("cpp-app", resource.Name);
        Assert.Equal(expectedWorkingDirectory, resource.WorkingDirectory);
        Assert.Equal(expectedExecutablePath, resource.Command);

        IList<string> args = await resource.GetArgumentListAsync();
        Assert.Equal(["--demo"], args);
    }

    [Fact]
    public void AddCppExecutableWithRootedExecutablePathDoesNotCombineWithWorkingDirectory()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        string rootedExecutablePath = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "tools", "cpp-app.exe"));

        builder.AddCppExecutable("cpp-app", "cpp-app", rootedExecutablePath);

        using DistributedApplication app = builder.Build();

        DistributedApplicationModel appModel = app.Services.GetRequiredService<DistributedApplicationModel>();
        CppAppExecutableResource resource = Assert.Single(appModel.Resources.OfType<CppAppExecutableResource>());

        Assert.Equal(rootedExecutablePath, resource.Command);
    }
}