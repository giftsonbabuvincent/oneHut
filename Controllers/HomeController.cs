using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using oneHut.Models;
using oneHut.ConnectDB;

namespace oneHut.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public const string SessionUserID = "_UserID";
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionUserID)))
        {
            ViewBag.pageName = "Booking";
        }
        return View();
    }

    public IActionResult Privacy()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "Booking";
        }
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID"))) { 
            ViewBag.pageName = "";
        }

        return View();
    }

    public IActionResult Login()
    {
        return View(new LoginModel());
    }

    [HttpPost]
     public IActionResult Login(LoginModel loginModel) 
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionUserID)))
        {
             HttpContext.Session.Remove(SessionUserID);
        }
        if(String.IsNullOrEmpty(loginModel.userName) || String.IsNullOrEmpty(loginModel.password)){
            loginModel.message = "Please enter login details!";
            return View(loginModel);
        }

        User user = new OneHutData().GetUser(
            new User() {Username = loginModel.userName, Password = loginModel.password});

        if(user != null) {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionUserID)))
            {
                HttpContext.Session.SetString(SessionUserID, user.UserID.ToString());
            }
            var name = HttpContext.Session.GetString(SessionUserID);
            return RedirectToAction("Booking","Booking");
        }
        
        loginModel.message = "Login failed!";
        return View(loginModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionUserID);
       return RedirectToAction("Login","Home");
    }
}
