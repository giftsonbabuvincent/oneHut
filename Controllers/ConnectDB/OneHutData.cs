using System;
using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using oneHut.Models;

namespace oneHut.ConnectDB;
public class OneHutData
{
    public string connectionString { get; set; } = @"mongodb://one-hut:ClX8trwKuFjdO9MUlfvk14bjzuPlbuG9M9SA86hWv5NKzV39kmbpr4bxZZ5OXgraRekSYarBCm1N3FMw5fTFzw==@one-hut.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@one-hut@";
    public MongoClient client { get; set; }
    public IMongoDatabase database { get; set; }
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

        List<Booking> collection = database.GetCollection<Booking>("Booking").Find(it => it.UserID.Equals(user.UserID)).ToList();

        if (!bookingModel.IsToday)
        {
            //filter guestname & phone
            if (!string.IsNullOrEmpty(bookingModel.Guest))
            {
                collection = collection.Where(it => it.GuestName.ToLower().Contains(bookingModel.Guest.ToLower()) || it.Phone.Contains(bookingModel.Guest)).ToList();
            }

            //filter checkin & checkout
            if (!string.IsNullOrEmpty(bookingModel.CheckIn) && !string.IsNullOrEmpty(bookingModel.CheckOut))
            {
                collection = collection.Where(it => Convert.ToDateTime(it.CheckIn.Trim().Substring(0, 10)).Date >= Convert.ToDateTime((bookingModel.CheckIn)).Date
                && Convert.ToDateTime(it.CheckOut.Trim().Substring(0, 10)).Date <= Convert.ToDateTime((bookingModel.CheckOut)).Date).ToList();
            }
            else
            {
                //filter checkin
                if (!string.IsNullOrEmpty(bookingModel.CheckIn))
                {
                    collection = collection.Where(it => it.CheckIn.Contains(bookingModel.CheckIn)).ToList();
                }

                //filter checkout
                if (!string.IsNullOrEmpty(bookingModel.CheckOut))
                {
                    collection = collection.Where(it => it.CheckOut.Contains(bookingModel.CheckOut)).ToList();
                }

            }
        }
        else
        {
            collection = collection.Where(it => Convert.ToDateTime(it.CheckIn.Trim().Substring(0, 10)).Date == DateTime.Now.Date
                || Convert.ToDateTime(it.CheckOut.Trim().Substring(0, 10)).Date == DateTime.Now.Date).ToList();
        }


        bookingModel.Bookings = new List<Booking>();

        int rowNo = 0;
        foreach (var book in collection.AsQueryable().OrderBy(it => it._id).TakeLast(100))
        {
            book.No = Convert.ToString(rowNo += 1);
            bookingModel.Bookings.Add(book);
        }
        return bookingModel;
    }

    public User GetUser(User user)
    {
        var collection = database.GetCollection<User>("User").Find(it => it.Username.Equals(user.Username) && it.Password.Equals(user.Password)).ToList();
        return collection.FirstOrDefault();
    }

    // insert booking
    public BookingModel AddBooking(BookingModel bookingModel, User user)
    {
        if (!String.IsNullOrEmpty(bookingModel.Book._id))
        {
            var collection = database.GetCollection<Booking>("Booking");
            var filter = Builders<Booking>.Filter.Eq("_id", bookingModel.Book._id.ToString());

            var update = Builders<Booking>.Update
                .Set("GuestName", bookingModel.Book.GuestName)
                .Set("Phone", bookingModel.Book.Phone)
                .Set("CheckIn", bookingModel.Book.CheckIn.Trim())
                .Set("CheckOut", bookingModel.Book.CheckOut.Trim())
                .Set("Rooms", bookingModel.Book.Rooms);

            var result = database.GetCollection<Booking>("Booking").UpdateOne(filter, update, null);

            return new BookingModel() { Message = "Updated Successfully" };
        }
        else
        {
            database.GetCollection<Booking>("Booking").InsertOne(new Booking()
            {
                UserID = bookingModel.Book.UserID,
                GuestName = bookingModel.Book.GuestName.Trim(),
                Phone = bookingModel.Book.Phone.Trim(),
                CheckIn = Convert.ToDateTime(bookingModel.Book.CheckIn.Trim()).ToString("MM/dd/yyyy") + " 00:00 00",
                CheckOut = Convert.ToDateTime(bookingModel.Book.CheckOut.Trim()).ToString("MM/dd/yyyy") + " 00:00 00",
                Rooms = bookingModel.Book.Rooms.Trim(),
                Status = bookingModel.Book.Status.Trim(),
            });
        }
        return bookingModel;
    }


    public BookingModel UpdateBooking(string useraction, string _id, User user)
    {
        var collection = database.GetCollection<Booking>("Booking");
        //Hard coded for testing
        var filter = Builders<Booking>.Filter.Eq("_id", _id.ToString());

        var update = Builders<Booking>.Update.Set("_id", _id.ToString());
        if (useraction.Equals("checkin"))
        {
            update = Builders<Booking>.Update
            .Set("CheckIn", DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
            .Set("Status", "CheckedIn");
        }

        else if (useraction.Equals("checkout"))
        {
            update = Builders<Booking>.Update
            .Set("CheckOut", DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
            .Set("Status", "Stayed");
        }

        else if (useraction.Equals("cancel"))
        {
            update = Builders<Booking>.Update.Set("Status", "Cancelled");
        }

        var result = database.GetCollection<Booking>("Booking").UpdateOne(filter, update, null);

        return new BookingModel() { Message = "Updated Successfully" };
    }
}