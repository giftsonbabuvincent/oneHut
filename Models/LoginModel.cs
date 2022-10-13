namespace oneHut.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class LoginModel
{
   public string userName {get;set;} = string.Empty;
   public string password {get;set;} = string.Empty;
   public string message {get;set;} = string.Empty;
   public string isAzureStorage  { get; set; } = string.Empty;
    public string UserGroupID { get; set; } = string.Empty;
    public string IsReadOnly { get; set; } = string.Empty;

}
public class User
{
    [BsonRepresentation(BsonType.ObjectId)] 
    public string _id { get; set; } = string.Empty;
    public string UserID { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string isAzureStorage  { get; set; } = string.Empty;
    public string UserGroupID { get; set; } = string.Empty;
    public string IsReadOnly { get; set; } = string.Empty;

}