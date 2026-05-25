using CommunityToolkit.Aspire.Testing;

namespace CommunityToolkit.Aspire.Hosting.Cpp.Tests;

public class AppHostTests(AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_Cpp_AppHost> fixture)
    : IClassFixture<AspireIntegrationTestFixture<Projects.CommunityToolkit_Aspire_Hosting_Cpp_AppHost>>
{
    [Fact]
    [Trait("Category", "Integration")]
    public void AppHostRegistersNoCppResourcesUntilExampleIsAdded()
    {
        DistributedApplicationModel model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();

        Assert.Empty(model.Resources.OfType<CppAppExecutableResource>());
    }
}