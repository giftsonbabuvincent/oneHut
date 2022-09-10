namespace oneHut.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class LoginModel
{
   public string userName {get;set;} = string.Empty;
   public string password {get;set;} = string.Empty;
   public string message {get;set;} = string.Empty;

}