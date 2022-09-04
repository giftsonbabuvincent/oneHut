using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Security.Authentication;

namespace oneHut.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    public string? username {get;set;}
    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
       
    }
}