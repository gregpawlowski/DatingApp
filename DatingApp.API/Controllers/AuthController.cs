using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        // Need to inject our IAuthRepository, so we can use our methods.
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            // Check if user exists by quering our repo
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists!");

            // Create a user so we can pass it to our Register
            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };
            // Register repo will craete a new user.
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            // We shoul be using CreatedAtRoute but we don't have a route yet.
            // return CreatedAtRoute()
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto) 
        {
            // User our login repo.
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null) 
                return Unauthorized();
            
            // Here we will generate the token
            // First create the payload which will include the ID and the Name of the user, we use Claims which are a different method of authorizatin then roles.
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            // Create a secret key, store the key in app settings.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            // Get the Sign in credentials fir the signature of the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create a security token descripter
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}