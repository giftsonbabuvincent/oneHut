using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Security.Authentication;
namespace oneHut.Pages;

public class BookingModel : PageModel
{
     private readonly ILogger<BookingModel> _logger;
    public List<Booking> Bookings {get;set;}
 public BookingModel(ILogger<BookingModel> logger)
    {
        Bookings = GetBookings();
    }
    public List<Booking>  GetBookings()
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
        var collection = database.GetCollection<Booking>("Booking");
        // Console.Write("HI");
        List<Booking> collections = new List<Booking>();
            foreach (var collection1 in collection.AsQueryable())
            {
                collections.Add(collection1);
            }
        return collections;
    }

}
public class Booking
{
    [BsonRepresentation(BsonType.ObjectId)] 
    public string _id { get; set; } = null!;
    public string No { get; set; } = null!;
    public string GuestName { get; set; } = null!;

    public string Phone { get; set; } = null!;
    public string CheckIn { get; set; } = null!;
    public string CheckOut { get; set; } = null!;
    public string Rooms { get; set; } = null!;
    public string Status { get; set; } = null!;

}