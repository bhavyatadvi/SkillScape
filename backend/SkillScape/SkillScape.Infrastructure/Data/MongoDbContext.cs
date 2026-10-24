using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SkillScape.Application.Configuration;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IMongoDatabase database, IOptions<MongoDbSettings> options)
    {
        _database = database;
        _settings = options.Value;
        EnsureIndexes();
    }

    public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>(_settings.UsersCollectionName);

    private void EnsureIndexes()
    {
        try
        {
            var userCollection = Users;
            var indexKeys = Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true, Name = "Idx_User_Email" };
            var indexModel = new CreateIndexModel<ApplicationUser>(indexKeys, indexOptions);
            userCollection.Indexes.CreateOne(indexModel);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"MongoDB EnsureIndexes failed: {ex.Message}. Continuing without index creation.");
        }
    }
}
