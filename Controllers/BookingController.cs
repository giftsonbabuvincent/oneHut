using System.Diagnostics;
using System.Globalization;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using oneHut.ConnectDB;
using oneHut.Models;
using System.Threading;

namespace oneHut.Controllers;

public class BookingController : Controller
{
    private readonly ILogger<BookingController> _logger;

    public BookingController(ILogger<BookingController> logger)
    {
        _logger = logger;
    }

    private void validateDate(string Date)
    {
        DateTimeStyles styles;
        DateTime dateResult;

        // Parse a date and time with no styles.
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        styles = DateTimeStyles.None;


        if (Date.Contains("00:00 00")) { if (!DateTime.TryParseExact(Date.Substring(0, 10), "dd/MM/yyyy", Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out dateResult)) { throw new Exception(); } }
        else { if (!DateTime.TryParse(Date, Thread.CurrentThread.CurrentCulture, styles, out dateResult)) { throw new Exception(); } }


    }

    [HttpGet]
    public IActionResult Booking(string pageNo)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }
        ViewBag.pageName = "Booking";
        return View(new OneHutData().GetBookings(
             new BookingModel()
             {
                 CurrentPage = string.IsNullOrEmpty(pageNo) ? 1 : Convert.ToInt32(pageNo),
                 Guest = HttpContext.Session.GetString("_Guest"),
                 CheckIn = HttpContext.Session.GetString("_CheckIn"),
                 CheckOut = HttpContext.Session.GetString("_CheckOut"),
                 IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
             },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") }));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult Booking(BookingModel bookingModel)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }
        // add booking
        OneHutData oneHutData = new OneHutData();
        String id = bookingModel.Book._id;
        Regex regex = new Regex(@"\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}\s+(AM|PM)");

        if (String.IsNullOrEmpty(id))
        {
            HttpContext.Session.SetString("_Guest", string.Empty);
            HttpContext.Session.SetString("_CheckIn", string.Empty);
            HttpContext.Session.SetString("_CheckOut", string.Empty);
            HttpContext.Session.SetString("_IsToday", "false");
            HttpContext.Session.SetString("_CurrentPage", "1");
            bookingModel.Book.UserID = HttpContext.Session.GetString("_UserID");
            bookingModel.Book.Status = "Booked";
        }
        try
        {
            validateDate(bookingModel.Book.CheckIn.ToUpper());
            validateDate(bookingModel.Book.CheckOut.ToUpper());

            bookingModel.Book.CheckIn = bookingModel.Book.CheckIn.ToUpper();
            bookingModel.Book.CheckOut = bookingModel.Book.CheckOut.ToUpper();

            oneHutData.AddBooking(bookingModel, new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        }
        catch (Exception e)
        {

            //Retrive Booking
            BookingModel _exbookingModel = new BookingModel();

            _exbookingModel.Guest = HttpContext.Session.GetString("_Guest");
            _exbookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn");
            _exbookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
            _exbookingModel.CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage"));
            _exbookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"));

            //Retrive Booking
            bookingModel = oneHutData.GetBookings(
            _exbookingModel,
                new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
            bookingModel.Book._id = id;
            bookingModel.Message = "Error saving data!";
            ViewBag.pageName = "Booking";
            return View(bookingModel);

        }
        ModelState.Clear();

        BookingModel _bookingModel = new BookingModel();

        _bookingModel.Guest = HttpContext.Session.GetString("_Guest");
        _bookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn");
        _bookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
        _bookingModel.CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage"));
        _bookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"));

        //Retrive Booking
        bookingModel = oneHutData.GetBookings(
           _bookingModel,
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        bookingModel.Book = new Booking();
        if (!String.IsNullOrEmpty(id)) { bookingModel.Message = "Booking updated successfully!"; }
        else { bookingModel.Message = "Booking successful!"; }
        ViewBag.pageName = "Booking";

        return View(bookingModel);

    }

    [HttpGet]
    public IActionResult UpdateBooking(string useraction, string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        OneHutData oneHutData = new OneHutData();

        oneHutData.UpdateBooking(useraction, id, new Models.User());

        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                Guest = HttpContext.Session.GetString("_Guest"),
                CheckIn = HttpContext.Session.GetString("_CheckIn"),
                CheckOut = HttpContext.Session.GetString("_CheckOut"),
                CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage")),
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpGet]
    public IActionResult EditBooking(string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        ModelState.Clear();
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                Guest = HttpContext.Session.GetString("_Guest"),
                CheckIn = HttpContext.Session.GetString("_CheckIn"),
                CheckOut = HttpContext.Session.GetString("_CheckOut"),
                CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage")),
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        bookingModel.Book = bookingModel.Bookings.Find(it => it._id.Equals(id));
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpGet]
    public IActionResult ClearBooking()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        ModelState.Clear();
        HttpContext.Session.SetString("_Guest", string.Empty);
        HttpContext.Session.SetString("_CheckIn", string.Empty);
        HttpContext.Session.SetString("_CheckOut", string.Empty);
        HttpContext.Session.SetString("_CurrentPage", "1");
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpPost]
    public IActionResult SearchBooking(BookingModel bookingModel)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }
        HttpContext.Session.SetString("_Guest", string.Empty);
        HttpContext.Session.SetString("_CheckIn", string.Empty);
        HttpContext.Session.SetString("_CheckOut", string.Empty);
        HttpContext.Session.SetString("_CurrentPage", "1");
        bookingModel.CurrentPage = 1;
        if (!string.IsNullOrEmpty(bookingModel.CheckIn))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            bookingModel.CheckIn = Convert.ToDateTime(bookingModel.CheckIn.Trim()).ToString("dd/MM/yyyy").ToUpper();
            HttpContext.Session.SetString("_CheckIn", bookingModel.CheckIn.ToString());
        }

        if (!string.IsNullOrEmpty(bookingModel.CheckOut))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            bookingModel.CheckOut = Convert.ToDateTime(bookingModel.CheckOut.Trim()).ToString("dd/MM/yyyy").ToUpper();
            HttpContext.Session.SetString("_CheckOut", bookingModel.CheckOut);
        }

        if (!string.IsNullOrEmpty(bookingModel.Guest))
        {
            HttpContext.Session.SetString("_Guest", Convert.ToString(bookingModel.Guest));
        }
        HttpContext.Session.SetString("_IsToday", "false");
        bookingModel.IsToday = false;

        OneHutData oneHutData = new OneHutData();
        BookingModel _bookingModel = new BookingModel();
        _bookingModel = oneHutData.GetBookings(
            bookingModel,
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        ViewBag.pageName = "Booking";
        return View("Booking", _bookingModel);
    }

    [HttpGet]
    public IActionResult TodayBooking()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        if (HttpContext.Session.GetString("_IsToday") == "true")
        {
            HttpContext.Session.SetString("_IsToday", "false");
            HttpContext.Session.SetString("_Guest", string.Empty);
            HttpContext.Session.SetString("_CheckIn", string.Empty);
            HttpContext.Session.SetString("_CheckOut", string.Empty);
        }
        else
        {
            HttpContext.Session.SetString("_IsToday", "true");
            HttpContext.Session.SetString("_Guest", string.Empty);
            HttpContext.Session.SetString("_CheckIn", string.Empty);
            HttpContext.Session.SetString("_CheckOut", string.Empty);
        }

        ModelState.Clear();
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpPost]
    public IActionResult NextPageBooking(BookingModel bookingModel)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        HttpContext.Session.SetString("_CurrentPage", bookingModel.CurrentPage.ToString());
        bookingModel.Guest = HttpContext.Session.GetString("_Guest");
        bookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn");
        bookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
        bookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"));

        OneHutData oneHutData = new OneHutData();
        BookingModel _bookingModel = new BookingModel();
        bookingModel = oneHutData.GetBookings(
            bookingModel,
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

}
