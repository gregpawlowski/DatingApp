using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class AuthRepository : IAuthRepository
  {
    private readonly DataContext _context;
    public AuthRepository(DataContext context)
    {
      _context = context;

    }
    public async Task<User> Login(string username, string password)
    {
        // Get the user
        var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
            return null;
        }

        return user;


    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        // Create new instance of hmac, this type pasing in the password salt from the database as the key.
        using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) 
        {
            // Get the computed hash for the apssword the user passed in, 
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            // The computedHash returned will be a byte array so we have to compare it in a for loop.
            for (int i = 0; i < computedHash.Length; i++) {
                if (computedHash[i] != passwordHash[i]) {
                    return false;
                }
            }
        }
        return true;
    }

    public async Task<User> Register(User user, string password)
    {
      byte[] passwordHash, passwordSalt;

      // Pass a reference so we can update the passwordHash and Salt.  
      CreatePasswordHash(password, out passwordHash, out passwordSalt);

      user.PasswordHash = passwordHash;
      user.PasswordSalt = passwordSalt;

      await _context.Users.AddAsync(user);

      await _context.SaveChangesAsync();

      return user;
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        // hmac is a disposable object, need to wrap it in using.
        using(var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            // The Salt is randomly generated, we can grab the salt frim this hmac object for storing in the database.
            passwordSalt = hmac.Key;
            // to compute a Hash from the current salt we have to pass in a byte Array. So we can use System.Text.Encoding.UTF8.GetBytes to change the user's password to a byte [].
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    public async Task<bool> UserExists(string username)
    {
        if (await _context.Users.AnyAsync(x => x.Username == username))
            return true;
    
        return false;
    }
  }
}