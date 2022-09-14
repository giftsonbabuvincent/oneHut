using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using oneHut.ConnectDB;
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
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index","Home");
        }
        ViewBag.pageName = "Booking";
        return View(new OneHutData().GetBookings(
            new BookingModel(),
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")}));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult Booking(BookingModel bookingModel) 
    {
        // add booking
        OneHutData oneHutData = new OneHutData();
        String id = bookingModel.Book._id;
        if(String.IsNullOrEmpty(id))
        {
            bookingModel.Book.UserID = HttpContext.Session.GetString("_UserID");
            bookingModel.Book.Status = "Booked";
        }
        oneHutData.AddBooking(bookingModel, new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        ModelState.Clear();

        //Retrive Booking
        bookingModel = oneHutData.GetBookings(
            new BookingModel(),
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        bookingModel.Book = new Booking();
        if(!String.IsNullOrEmpty(id)) { bookingModel.Message = "Booking updated successfully!"; }
        else{ bookingModel.Message = "Booking successful!"; }
        ViewBag.pageName = "Booking";

        return View(bookingModel);

    }

    [HttpGet]
    public IActionResult UpdateBooking(string useraction, string id) 
    {
        new OneHutData().UpdateBooking(useraction,id,new Models.User());
        return RedirectToAction("Booking","Booking");
    }

    [HttpGet]
    public IActionResult EditBooking(string id) 
    {
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel(),
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        bookingModel.Book = bookingModel.Bookings.Find(it=>it._id.Equals(id));
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

     [HttpGet]
    public IActionResult ClearBooking() 
    {
        ModelState.Clear();
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel(),
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }
}
