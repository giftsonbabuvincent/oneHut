namespace oneHut.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class BookingModel
{
    public List<Booking> Bookings { get; set; }

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
