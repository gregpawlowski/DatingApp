using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))]
  [Authorize]
  [Route("api/[controller]")]
  public class UsersController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    public UsersController(IDatingRepository repo, IMapper mapper)
    {
      _mapper = mapper;
      _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
    {
      //Get currently loggedin UserID
      var currentUserId =  int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

      // Get currently Logged in user so we can know the gender etc.
      var userFromRepo = await _repo.GetUser(currentUserId);
      
      userParams.UserId = currentUserId;

      // If we don't have a gender in the params we have to set it to whetever the opposite is of the currently logged in user 
      if (string.IsNullOrEmpty(userParams.Gender)) 
      {
        userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
      }

      var users = await _repo.GetUsers(userParams);

      var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

      Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

      return Ok(usersToReturn);
    }

    [HttpGet("{id}", Name="GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await _repo.GetUser(id);

      var userToReturn = _mapper.Map<UserForDetailsDto>(user);

      return Ok(userToReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody]UserForUpdateDto userForUpdateDto)
    {
      // Compare that the id the user wants to update matches the id that was passed in the token.
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();
      
      var userFromRepo = await _repo.GetUser(id);

      _mapper.Map(userForUpdateDto, userFromRepo);

      if (await _repo.SaveAll())
        return NoContent();
      
      throw new Exception($"Updating user {id} failed on save");
    }
  }
}