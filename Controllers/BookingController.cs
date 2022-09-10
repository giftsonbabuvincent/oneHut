using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using oneHut.Models;

namespace oneHut.Controllers;

public class BookingController : Controller
{
    private readonly ILogger<BookingController> _logger;

    public BookingController(ILogger<BookingController> logger)
    {
        _logger = logger;
    }

    public IActionResult Booking()
    {
        return View(GetBookings(new BookingModel()));
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public BookingModel GetBookings(BookingModel bookingModel)
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
        bookingModel.Bookings = new List<Booking>();
        foreach (var book in collection.AsQueryable())
        {
            bookingModel.Bookings.Add(book);
        }
        return bookingModel;
    }
    public IActionResult AddBooking()
    {
        BookingModel bookingModel = new BookingModel();
        bookingModel.Book = new Booking(){GuestName="giftson"};
        return PartialView("AddBooking",bookingModel);
    }
    public IActionResult AllBookings()
    {
        return PartialView("AllBookings",GetBookings(new BookingModel()));
    }

}
