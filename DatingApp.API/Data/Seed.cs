using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
  public class Seed
  {
    // private readonly DataContext _context;
    // public Seed(DataContext context)
    // {
    //   _context = context;
    // }

    public static void SeedUsers(DataContext context)
    {

      // Check if users exist in databse
      if (!context.Users.Any())
      {
        // Read all data.
        var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");

        // Serialize the data
        var users = JsonConvert.DeserializeObject<List<User>>(userData);

        foreach (var user in users)
        {
          // Need to save in the DB the same way using our repository
          byte[] passwordHash, passwordSalt;
          CreatePasswordHash("password", out passwordHash, out passwordSalt);
          user.PasswordHash = passwordHash;
          user.PasswordSalt = passwordSalt;
          user.Username = user.Username.ToLower();

          context.Users.Add(user);
        }

        context.SaveChanges();
      }
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      // hmac is a disposable object, need to wrap it in using.
      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        // The Salt is randomly generated, we can grab the salt frim this hmac object for storing in the database.
        passwordSalt = hmac.Key;
        // to compute a Hash from the current salt we have to pass in a byte Array. So we can use System.Text.Encoding.UTF8.GetBytes to change the user's password to a byte [].
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }
  }
}