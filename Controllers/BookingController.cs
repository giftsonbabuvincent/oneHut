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
}
