using MongoDB.Driver;

namespace Stanmore.Repository.IntegrationTests;

public class PremiumUserRepositoryTests
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IMongoCollection<PremiumUser> _premiumUserCollection;

    private readonly string _databaseName = $"testdb_{Guid.NewGuid():N}";

    public PremiumUserRepositoryTests()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _mongoDatabase = client.GetDatabase(_databaseName);

        var databaseOptions = new DatabaseOptions()
        {
            Name = "Stanmore",
            Collection = "PremiumUsers",
            ConnectionString = "mongodb://localhost:27017"
        };

        var options = Microsoft.Extensions.Options.Options.Create(databaseOptions);

        _premiumUserCollection = _mongoDatabase.GetCollection<PremiumUser>(databaseOptions.Collection);
    }
    private async Task InitializeAsync()
    {
        await _mongoDatabase.Client.DropDatabaseAsync(_databaseName);
    }
}
