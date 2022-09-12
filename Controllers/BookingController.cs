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

        if(String.IsNullOrEmpty(bookingModel.Book._id))
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
        bookingModel.Message = "Booking Successful!";
        ViewBag.pageName = "Booking";

        return View(bookingModel);

    }

    [HttpGet]
    public IActionResult CheckinBooking(string id) 
    {
        return RedirectToAction("Booking","Booking");
    }

   [HttpGet]
    public IActionResult CheckoutBooking(string id) 
    {
        return RedirectToAction("Booking","Booking");
    }

    [HttpGet]
    public IActionResult CancelBooking(string id) 
    {
        return RedirectToAction("Booking","Booking");
    }

}
