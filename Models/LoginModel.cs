namespace oneHut.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class LoginModel
{
   public string userName {get;set;}
   public string password {get;set;}
   public string message {get;set;}

}