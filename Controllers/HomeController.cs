using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using oneHut.Models;

namespace oneHut.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View(new LoginModel());
    }

    [HttpPost]
     public IActionResult Login(LoginModel loginModel) 
    {
        if(String.IsNullOrEmpty(loginModel.userName) || String.IsNullOrEmpty(loginModel.password)){
        loginModel.message = "Please enter login details!";
        return View(loginModel);
        }
        // return RedirectToAction("Booking","Booking");
        if(loginModel.userName.Equals("giftson") && loginModel.password.Equals("password1")) {
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
}
