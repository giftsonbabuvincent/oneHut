using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using oneHut.Models;

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
            return RedirectToAction("Index","Home");}

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

        string connectionString = 
        @"mongodb://one-hut:ClX8trwKuFjdO9MUlfvk14bjzuPlbuG9M9SA86hWv5NKzV39kmbpr4bxZZ5OXgraRekSYarBCm1N3FMw5fTFzw==@one-hut.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@one-hut@";
        MongoClientSettings settings = MongoClientSettings.FromUrl(
        new MongoUrl(connectionString)
        );
    
        settings.SslSettings = 
        new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
        MongoClient client = new MongoClient(settings);
        
        var database = client.GetDatabase("OneHutDB"); 
        var collection = database.GetCollection<User>("User").Find(it=>it.Username.Equals(loginModel.userName) && it.Password.Equals(loginModel.password)).ToList();
        
        if(collection.Count == 1) {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionUserID)))
            {
                HttpContext.Session.SetString(SessionUserID, collection.FirstOrDefault().UserID.ToString());
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
