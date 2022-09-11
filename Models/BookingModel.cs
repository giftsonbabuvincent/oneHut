namespace oneHut.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class BookingModel
{
    public Booking Book {get;set;} = new Booking();
    public List<Booking> Bookings { get; set; } = new List<Booking>();

}
public class Booking
{
    [BsonRepresentation(BsonType.ObjectId)] 
    public string _id { get; set; } = string.Empty;
    public string UserID { get; set; } = string.Empty;
    public string No { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public string Rooms { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

}
