using System;
using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using oneHut.Models;

namespace oneHut.ConnectDB;
public class OneHutData
{
    public string connectionString{get;set;} = @"mongodb://one-hut:ClX8trwKuFjdO9MUlfvk14bjzuPlbuG9M9SA86hWv5NKzV39kmbpr4bxZZ5OXgraRekSYarBCm1N3FMw5fTFzw==@one-hut.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@one-hut@";
    public MongoClient client {get;set;}
    public IMongoDatabase database {get;set;}
    public OneHutData()
    {
        MongoClientSettings settings = MongoClientSettings.FromUrl(
        new MongoUrl(connectionString)
        );
    
        settings.SslSettings = 
        new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

        client = new MongoClient(settings);
        database = client.GetDatabase("OneHutDB");
    }

    public BookingModel GetBookings(BookingModel bookingModel, User user)
    {
        
        var collection = database.GetCollection<Booking>("Booking").Find(it=>it.UserID.Equals(user.UserID)).ToList();
        bookingModel.Bookings = new List<Booking>();
        foreach (var book in collection.AsQueryable())
        {
            book.CheckIn = Convert.ToDateTime(book.CheckIn).ToString("dd/MM/yyyy hh:mm tt");
            book.CheckOut = Convert.ToDateTime(book.CheckOut).ToString("dd/MM/yyyy hh:mm tt");
            
            bookingModel.Bookings.Add(book);
        }
        return bookingModel;
    }

    public User GetUser(User user)
    {
        var collection = database.GetCollection<User>("User").Find(it=>it.Username.Equals(user.Username) && it.Password.Equals(user.Password)).ToList();
        return collection.FirstOrDefault();
    }
} 