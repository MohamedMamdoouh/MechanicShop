using Xunit;
namespace MechanicShop.Api.IntegrationTests.Common;

[CollectionDefinition(CollectionName)]
public class WebFactoryCollection : ICollectionFixture<WebFactory>
{
    public const string CollectionName = "WebFactory Collection";
}
