using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [AllowAnonymous]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;
    // Need to inject our IAuthRepository, so we can use our methods.
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManger;
    public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManger)
    {
      _signInManger = signInManger;
      _userManager = userManager;
      _mapper = mapper;
      _config = config;
      _repo = repo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
    {
      // // IDentity has a normalized username taht it checks against.
      // userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

      // // UserManagear already check thsi by default
      // // Check if user exists by quering our repo
      // if (await _repo.UserExists(userForRegisterDto.Username))
      //   return BadRequest("Username already exists!");

      // Create a user so we can pass it to our Register
      var userToCreate = _mapper.Map<User>(userForRegisterDto);

      var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

      // Register repo will craete a new user.
      // var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

      var userToReturn = _mapper.Map<UserForDetailsDto>(userToCreate);

      if (result.Succeeded)
      {
        // We shoul be using CreatedAtRoute but we don't have a route yet.
        // return StatusCode(201);
        return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
      }

      return BadRequest(
        // Take only the description and return the string from that.
        string.Join("",
          from error in result.Errors
          select error.Description
        )
      );

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
      // User our login repo.
      // var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

      // Find the user using the userManager, find by username
      var user = await _userManager.FindByNameAsync(userForLoginDto.Username);

      if (user == null)
        return Unauthorized();

      // Sign in user, but don't lock the user out on failure.
      var result = await _signInManger.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

      // if (userFromRepo == null)
      //   return Unauthorized();

      if (result.Succeeded)
      {
        var userToSendDown = _mapper.Map<UserForListDto>(user);

        return Ok(new
        {
          token = await GenerateJWTToken(user),
          user = userToSendDown
        });
      }

      return Unauthorized();
    }

    private async Task<string> GenerateJWTToken(User user)
    {
      // Here we will generate the token
      // First create the payload which will include the ID and the Name of the user, we use Claims which are a different method of authorizatin then roles.
      var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

      // Get all the roles
      var roles = await _userManager.GetRolesAsync(user);

      // Add all the roles to the token
      foreach(var role in roles)
      {
        // Converted the claims from an array to a List so we can add roles on it
        claims.Add(new Claim(ClaimTypes.Role, role));
      }

      // Create a secret key, store the key in app settings.
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

      // Get the Sign in credentials fir the signature of the token.
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      // Create a security token descripter
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        NotBefore = DateTime.Now,
        SigningCredentials = creds
      };

      var tokenHandler = new JwtSecurityTokenHandler();

      var token = tokenHandler.CreateToken(tokenDescriptor);

      return tokenHandler.WriteToken(token);

    }
  }
}