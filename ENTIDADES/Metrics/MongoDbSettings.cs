namespace POS3.Data.Mongo;

public class MongoDbSettings
{
    public const string SectionName = "MongoDBSettings";

    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
}

