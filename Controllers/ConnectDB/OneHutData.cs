using System;
using System.Globalization;
using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using oneHut.Models;
using System.Threading;
using Azure.Storage.Files.Shares;
using Azure;
using Microsoft.AspNetCore.Hosting;
using Azure.Storage.Files.Shares.Models;

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

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
    }

    public BookingModel GetBookings(BookingModel bookingModel, User user)
    {

        List<Booking> collection = database.GetCollection<Booking>("Booking").Find(it => it.UserID.Equals(user.UserID)).ToList();

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

        if (bookingModel.IsToday)
        {
            collection = collection.Where(it => Convert.ToDateTime(it.CheckIn.Trim().Substring(0, 10)).Date == DateTime.Now.Date
                || Convert.ToDateTime(it.CheckOut.Trim().Substring(0, 10)).Date == DateTime.Now.Date).ToList();
        }


        bookingModel.Bookings = new List<Booking>();
        if (collection.Count > bookingModel.TakeItem)
        {
            int TotalItems = bookingModel.TotalPages = collection.Count;
            bookingModel.Pages = new List<int>();

            for (int i = 1; i <= Math.Round((TotalItems / Convert.ToDouble(bookingModel.TakeItem)), MidpointRounding.ToPositiveInfinity); i++)
            {
                bookingModel.Pages.Add(i);
            }

        }
        int rowNo = 0;
        foreach (var book in collection.AsQueryable().OrderByDescending(it => it._id)
        .Skip(bookingModel.TakeItem * (bookingModel.CurrentPage - 1))
        .Take(bookingModel.TakeItem))
        {
            book.No = Convert.ToString(rowNo += 1);
            book.BillAmount = string.IsNullOrEmpty(book.BillAmount) ? string.Empty : "₹" + string.Format("{0:#.00}", book.BillAmount);
            book.AmountPaid = string.IsNullOrEmpty(book.AmountPaid) ? string.Empty : "₹" + string.Format("{0:#.00}", book.AmountPaid);
            book.PaymentStatus = PaymentStatus(new string[] { book.BillAmount, book.AmountPaid });
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
                .Set("Rooms", bookingModel.Book.Rooms)
                .Set("Rating", bookingModel.Book.Rating)
                .Set("AdditionalInfo", bookingModel.Book.AdditionalInfo)
                .Set("BillAmount", string.Format("{0:#.00}", bookingModel.Book.BillAmount))
                .Set("AmountPaid", string.Format("{0:#.00}", bookingModel.Book.AmountPaid))
                // .Set("PaymentStatus", PaymentStatus(new string[] {bookingModel.Book.BillAmount.Replace("₹", string.Empty), bookingModel.Book.AmountPaid.Replace("₹", string.Empty)}))
                .Set("ActionDateTime", bookingModel.Book.ActionDateTime);


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
                CheckIn = Convert.ToDateTime(bookingModel.Book.CheckIn.Trim()).ToString("dd/MM/yyyy") + " 00:00 00",
                CheckOut = Convert.ToDateTime(bookingModel.Book.CheckOut.Trim()).ToString("dd/MM/yyyy") + " 00:00 00",
                Rooms = bookingModel.Book.Rooms.Trim(),
                Status = bookingModel.Book.Status.Trim(),
                Rating = bookingModel.Book.Rating,
                AdditionalInfo = bookingModel.Book.AdditionalInfo,
                BillAmount = string.Format("{0:#.00}", bookingModel.Book.BillAmount),
                AmountPaid = string.Format("{0:#.00}", bookingModel.Book.AmountPaid),
                // PaymentStatus = bookingModel.Book.PaymentStatus,
                ActionDateTime = bookingModel.Book.ActionDateTime
            });

            // Get inserted _id
            Booking collection = database.GetCollection<Booking>("Booking").Find(it => it.UserID.Equals(user.UserID)
            && it.GuestName.Equals(bookingModel.Book.GuestName.Trim())
            && it.Phone.Equals(bookingModel.Book.Phone.Trim())
            && it.ActionDateTime.Equals(bookingModel.Book.ActionDateTime)
            ).ToList().LastOrDefault();

            bookingModel.Book._id = collection._id;

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
            .Set("CheckIn", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt").ToUpper())
            .Set("Status", "CheckedIn");
        }

        else if (useraction.Equals("checkout"))
        {
            update = Builders<Booking>.Update
            .Set("CheckOut", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt").ToUpper())
            .Set("Status", "Stayed");
        }

        else if (useraction.Equals("cancel"))
        {
            update = Builders<Booking>.Update.Set("Status", "Cancelled");
        }

        var result = database.GetCollection<Booking>("Booking").UpdateOne(filter, update, null);

        return new BookingModel() { Message = "Updated Successfully" };
    }



    private string PaymentStatus(string[] amount)
    {
        if (amount.FirstOrDefault().Length > 0 && string.IsNullOrEmpty(amount.LastOrDefault())) { return "Unpaid"; }
        if (amount.FirstOrDefault().Length > 0 && amount.FirstOrDefault() == amount.LastOrDefault()) { return "Paid"; }
        if (amount.FirstOrDefault().Length > 0 && Convert.ToDouble(amount.FirstOrDefault().TrimStart('₹')) < Convert.ToDouble(amount.LastOrDefault().TrimStart('₹'))) { return "Excess Paid"; }
        if (amount.FirstOrDefault().Length > 0 && amount.FirstOrDefault() != amount.LastOrDefault()) { return "Partially Paid"; }

        return string.Empty;
    }


    public void AzureUploadFile(List<string> postedFiles, string strdirectory, string wwwroortpath)
    {
        var connectionString = "DefaultEndpointsProtocol=https;AccountName=onehut;AccountKey=tWDrTIbkgFQQOqYsaD3HLPyYVT0EdtmSl/NKuclnnqPfaErjClWB04RSr52DlVet1rTN1gTHE76c+AStpJVrSw==;EndpointSuffix=core.windows.net";
        var fileShareName = "onehutfileshare";
        var fileName = "";

        // Create Directory
        // Get a reference to a directory and create it
        string[] arrayPath = strdirectory.Split('/');
        string buildPath = string.Empty;
        ShareClient share = new(connectionString, fileShareName);

        ShareDirectoryClient createDirectory = null; // share.GetDirectoryClient(dirName);
                                                     // Here's goes the nested directories builder
        for (int i = 0; i < arrayPath.Length; i++)
        {
            buildPath += arrayPath[i];
            createDirectory = share.GetDirectoryClient(buildPath);
            createDirectory.CreateIfNotExists();
            buildPath += '/';
        }

        List<string> uploadedFiles = new List<string>();
        int fileCount = 1;
        foreach (string postedFile in postedFiles)
        {
            fileName = postedFile.Split("/").LastOrDefault();

            var directory = share.GetDirectoryClient(strdirectory);

            var file = directory.GetFileClient(fileName);
            using FileStream stream = File.OpenRead(wwwroortpath.Replace("\\", "/") + "/" + fileName);

            file.Create(stream.Length);
            file.UploadRange(
                new HttpRange(0, stream.Length),
                stream);


            fileCount += 1;
        }



    }
    public List<string> AzureUploadedFile(string _id, string azurePath)
    {

        string connectionString = "DefaultEndpointsProtocol=https;AccountName=onehut;AccountKey=tWDrTIbkgFQQOqYsaD3HLPyYVT0EdtmSl/NKuclnnqPfaErjClWB04RSr52DlVet1rTN1gTHE76c+AStpJVrSw==;EndpointSuffix=core.windows.net";
        string shareName = "onehutfileshare";
        
        ShareClient share = new ShareClient(connectionString, shareName);
        List<string> uploadedFiles = new List<string>();

        // Track the remaining directories to walk, starting from the root
        var remaining = new Queue<ShareDirectoryClient>();
        remaining.Enqueue(share.GetRootDirectoryClient());
        while (remaining.Count > 0)
        {
            // Get all of the next directory's files and subdirectories
            ShareDirectoryClient dir = remaining.Dequeue();
            foreach (ShareFileItem item in dir.GetFilesAndDirectories())
            {
                uploadedFiles.Add(item.Name);
            }
        }

        //uploadedFiles.Add("/Uploads/USR0001/632f16625f51c805bb23f952/HeartBox.jpg");
        return uploadedFiles;
    }

}