using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))]
  [Route("api/users/{userId}/[controller]")]
//   [ApiController]
  public class MessagesController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    public MessagesController(IDatingRepository repo, IMapper mapper)
    {
      _mapper = mapper;
      _repo = repo;
    }

    [HttpGet("{id}", Name="GetMessage")]
    public async Task<IActionResult> GetMessage(int userId, int id)
    {
        // Compare that the id the user wants to update matches the id that was passed in the token.
        if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            return Unauthorized();
        // Get message from Repo
        var messageFromRepo = await _repo.GetMessage(id);

        if (messageFromRepo == null)
            return NotFound();

        return Ok(messageFromRepo);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(int userId, [FromBody]MessageForCreationDto messageForCreationDto)
    {
        // Get the sender beucae we ned autoMapper to map the sender info later.
        var sender = await _repo.GetUser(userId);

        // Compare that the id the user wants to update matches the id that was passed in the token.
        if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            return Unauthorized();

        // Set the sender id to be the authorized user.
        messageForCreationDto.SenderId = userId;
        
        // Check if the user this user is trying to send this message to exists
        var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

        if (recipient == null)
            return BadRequest("Could not find user");

        var message = _mapper.Map<Message>(messageForCreationDto);

        _repo.Add(message);

        if(await _repo.SaveAll())
        {
          // Convert the message back into a DTO. Need to do this inside the if statement so that we get the ID after the message has been craeted.
          var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
          return CreatedAtRoute("GetMessage", new {userId, id = message.Id}, messageToReturn);
        }

        throw new Exception("Creating the message failed on save");
        // return BadRequest("Could not save message");
    }

    [HttpGet]
    public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
    {
      // Compare that the id the user wants to update matches the id that was passed in the token.
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
          return Unauthorized();

      // Need to add userId from the route into messageParams
      messageParams.UserId = userId;

      // Get the list of messages from the repo. messagesFromRepo returns a PageList so we have all the paging information in it.
      var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

      // Map into messages to Return
      var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

      // We need to return our pagination header.
      Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

      return Ok(messages);
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
    {
      // Compare that the id the user wants to update matches the id that was passed in the token.
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
          return Unauthorized();
      
      var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);

      var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

      return Ok(messageThread);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> DeleteMessage(int id, int userId)
    {
      // Compare that the id the user wants to update matches the id that was passed in the token.
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
          return Unauthorized();

      var messageFromRepo = await _repo.GetMessage(id);

      if (messageFromRepo.SenderId == userId)
        messageFromRepo.SenderDeleted = true;
      
      if (messageFromRepo.RecipientId == userId)
        messageFromRepo.RecipientDeleted = true;

      if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
        _repo.Delete(messageFromRepo);

      if (await _repo.SaveAll())
        return NoContent();
      
      throw new Exception("Error deleting the message");
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var message = await _repo.GetMessage(id);

      // Make sure the recipeint of this message is the one that is marking it read.
      if(message.RecipientId != userId)
        return Unauthorized();
        
      message.IsRead = true;
      message.DateRead = DateTime.Now;

      await _repo.SaveAll();
      
      return NoContent();

    }
  }

}