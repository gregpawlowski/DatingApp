using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
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


    // Instead of DataContext we have to use USer Manger.
    public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
    {

      // Check if users exist in databse
      if (!userManager.Users.Any())
      {
        // Read all data.
        var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");

        // Serialize the data
        var users = JsonConvert.DeserializeObject<List<User>>(userData);

        // Creare roles
        var roles = new List<Role>
        {
          new Role{Name = "Member"},
          new Role{Name = "Admin"},
          new Role{Name = "Moderator"},
          new Role{Name = "VIP"}
        };

        foreach (var role in roles)
        {
          roleManager.CreateAsync(role).Wait();
        }

        foreach (var user in users)
        {
          // // Need to save in the DB the same way using our repository
          // byte[] passwordHash, passwordSalt;
          // CreatePasswordHash("password", out passwordHash, out passwordSalt);
          // // user.PasswordHash = passwordHash;
          // // user.PasswordSalt = passwordSalt;
          // user.UserName = user.UserName.ToLower();

          // context.Users.Add(user);

          // Get the only photo for this user and set it to approved
          user.Photos.SingleOrDefault().IsApproved = true;

          // Create a user using the userManager, pass in the user and apsswrod and specify Wait() because this is a Async Method and we are ont in a async function.
          userManager.CreateAsync(user, "password").Wait();
          userManager.AddToRoleAsync(user, "Member").Wait();
        }

        // Create admin User
        var adminUser = new User {
          UserName = "Admin"
        };

        var result = userManager.CreateAsync(adminUser, "password").Result;

        if (result.Succeeded)
        {
          var admin = userManager.FindByNameAsync("Admin").Result;
          userManager.AddToRolesAsync(admin, new [] {"Moderator", "Admin"}).Wait();
        }
  

        // userManger automaticlaly cretes and save the user so SaveChanges() is not needed.
        // context.SaveChanges();
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