using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Security.Authentication;

namespace oneHut.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
public string? username {get;set;}
    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        // OnGetAsync();
         GetUsers();
         username = Convert.ToString(GetUsers().FirstOrDefault().username);
    }
    public List<User>  GetUsers()
    {
        //string connectionStringMongoDB = "mongodb+srv://giftson01:Wpg5mFxVOJyhs3sq@giftson-01.fpd9fbi.mongodb.net/";  
        // New instance of CosmosClient class
        // MongoClient client = new MongoClient(connectionString); 
            string connectionString = 
            @"mongodb://one-hut:ClX8trwKuFjdO9MUlfvk14bjzuPlbuG9M9SA86hWv5NKzV39kmbpr4bxZZ5OXgraRekSYarBCm1N3FMw5fTFzw==@one-hut.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@one-hut@";
            MongoClientSettings settings = MongoClientSettings.FromUrl(
            new MongoUrl(connectionString)
            );
            settings.SslSettings = 
            new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            MongoClient client = new MongoClient(settings);
        //MongoClient client = new MongoClient(connectionString,);
        var database = client.GetDatabase("OneHutDB"); 
        var collection = database.GetCollection<User>("user");
        // Console.Write("HI");
        List<User> collections = new List<User>();
            foreach (var collection1 in collection.AsQueryable())
            {
                collections.Add(collection1);
            }
        return collections;
    }
}
public class oneHutDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}
public class User
{
    [BsonRepresentation(BsonType.ObjectId)] 
    public string _id { get; set; } = null!;
    public string username { get; set; } = null!;

    public string password { get; set; } = null!;

}
/*
    public void OnGetAsync()
    {
    var builder = WebApplication.CreateBuilder();

    // Add services to the container.
    builder.Services.Configure<oneHutDatabaseSettings>(
        builder.Configuration.GetSection("oneHutDatabase"));
        oneHutDatabaseSettings _oneHutDatabaseSettings = new oneHutDatabaseSettings();
        _oneHutDatabaseSettings.ConnectionString ="mongodb+srv://giftson01:Wpg5mFxVOJyhs3sq@giftson-01.fpd9fbi.mongodb.net/";
        _oneHutDatabaseSettings.DatabaseName ="GiftsonDB";
        _oneHutDatabaseSettings.BooksCollectionName ="user";
        Task<User?> users = (new UserService(_oneHutDatabaseSettings)).GetAsync("giftson");
    }
    public void GetUsers()
    {
        string connectionString = "mongodb+srv://giftson01:Wpg5mFxVOJyhs3sq@giftson-01.fpd9fbi.mongodb.net/";  
        MongoClient client = new MongoClient(connectionString);  
        var database = client.GetDatabase("GiftsonDB"); 
        var collection = database.GetCollection<User>("user");
        // Console.Write("HI");
        List<User> collections = new List<User>();
            foreach (var collection1 in collection.AsQueryable())
            {
                collections.Add(collection1);
            }
        username = collections.FirstOrDefault().username;
    }
}
public class oneHutDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string BooksCollectionName { get; set; } = null!;
}
public class User
{
    [BsonRepresentation(BsonType.ObjectId)] 
    public string _id { get; set; } = null!;
    public string username { get; set; } = null!;

    public string password { get; set; } = null!;

}

public class UserService
{
    private readonly IMongoCollection<User> _booksCollection;

    public UserService(
        oneHutDatabaseSettings bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<User>(
            bookStoreDatabaseSettings.BooksCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
        await _booksCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string username) =>
        await _booksCollection.Find(x => x.username == username).FirstOrDefaultAsync();

    public async Task CreateAsync(User newBook) =>
        await _booksCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string username, User updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.username == username, updatedBook);

    public async Task RemoveAsync(string username) =>
        await _booksCollection.DeleteOneAsync(x => x.username == username);
}
*/