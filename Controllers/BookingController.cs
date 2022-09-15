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
             new BookingModel(){ CheckIn = HttpContext.Session.GetString("_CheckIn"), CheckOut = HttpContext.Session.GetString("_CheckOut"), IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday")) },
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
        try {
            oneHutData.AddBooking(bookingModel, new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        } catch (Exception e) {
            
                //Retrive Booking
            BookingModel _exbookingModel = new BookingModel();
           
            _exbookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn"); 
            _exbookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
            _exbookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday")); 

            //Retrive Booking
            bookingModel = oneHutData.GetBookings(
            _exbookingModel,
                new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
            bookingModel.Book = new Booking();
                    bookingModel.Message = "Error saving data!";
                return View(bookingModel);

        }
        ModelState.Clear();
        
        BookingModel _bookingModel = new BookingModel();
      
        _bookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn"); 
        _bookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
        _bookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday")); 
    
        //Retrive Booking
        bookingModel = oneHutData.GetBookings(
           _bookingModel,
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
        OneHutData oneHutData = new OneHutData();

        oneHutData.UpdateBooking(useraction,id,new Models.User());

        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel(){ CheckIn = HttpContext.Session.GetString("_CheckIn"), CheckOut = HttpContext.Session.GetString("_CheckOut"), IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday")) },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        bookingModel.Book = bookingModel.Bookings.Find(it=>it._id.Equals(id));
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpGet]
    public IActionResult EditBooking(string id) 
    {
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel(){ CheckIn = HttpContext.Session.GetString("_CheckIn"), CheckOut = HttpContext.Session.GetString("_CheckOut"), IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday")) },
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
            new BookingModel(){ CheckIn = HttpContext.Session.GetString("_CheckIn"), 
                                CheckOut = HttpContext.Session.GetString("_CheckOut"), 
                                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))},
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpPost]
    public IActionResult SearchBooking(BookingModel bookingModel) 
    {
        if(!string.IsNullOrEmpty(bookingModel.CheckIn))
        { 
            bookingModel.CheckIn = Convert.ToDateTime(bookingModel.CheckIn.Trim()).ToString("MM/dd/yyyy"); 
            HttpContext.Session.SetString("_CheckIn", bookingModel.CheckIn.ToString());
        }

        if(!string.IsNullOrEmpty(bookingModel.CheckOut))
        { 
            bookingModel.CheckOut = Convert.ToDateTime(bookingModel.CheckOut.Trim()).ToString("MM/dd/yyyy"); 
            HttpContext.Session.SetString("_CheckOut", bookingModel.CheckOut);
        }
        HttpContext.Session.SetString("_IsToday", "false");
        bookingModel.IsToday = false;

        OneHutData oneHutData = new OneHutData();
        BookingModel _bookingModel = new BookingModel();
        _bookingModel = oneHutData.GetBookings(
            bookingModel,
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        ViewBag.pageName = "Booking";
        return View("Booking", _bookingModel);
    }

    [HttpGet]
    public IActionResult TodayBooking() 
    {
        if(HttpContext.Session.GetString("_IsToday") == "true")
        {
            HttpContext.Session.SetString("_IsToday", "false"); 
        } 
        else
        {
            HttpContext.Session.SetString("_IsToday", "true"); 
        }
    
       ModelState.Clear();
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel(){ CheckIn = HttpContext.Session.GetString("_CheckIn"), 
                                CheckOut = HttpContext.Session.GetString("_CheckOut"), 
                                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))},
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID")});
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

}
