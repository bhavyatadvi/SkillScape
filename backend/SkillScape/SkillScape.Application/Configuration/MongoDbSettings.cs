namespace SkillScape.Application.Configuration;

/// <summary>
/// MongoDB connection settings
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "SkillScapeDb";
    public string UsersCollectionName { get; set; } = "Users";
}
