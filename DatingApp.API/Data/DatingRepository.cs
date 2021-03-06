using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class DatingRepository : IDatingRepository
  {
    private readonly DataContext _context;
    public DatingRepository(DataContext context)
    {
      _context = context;
    }
    public void Add<T>(T entity) where T : class
    {
      _context.Add(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
      _context.Remove(entity);
    }

    public async Task<User> GetUser(int id, bool isCurrentUser)
    {
			// Include photos
			// Default retunrs null if user isn't found.
      // var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

      var query = _context.Users.Include(p => p.Photos).AsQueryable();

      if (isCurrentUser)
        query = query.IgnoreQueryFilters();

      var user = await query.FirstOrDefaultAsync(u => u.Id == id);

			return user;
    }

    public async Task<PageList<User>> GetUsers(UserParams userParams)
    { 
      // Create an IQueriable of Users from the context but don't execute it yet.
      // Set it as a Queryable so we can add more where clauses
      var users = _context.Users.Include(p => p.Photos)
        // Add default ordering by Last Active descending.
        .OrderByDescending(u => u.LastActive).AsQueryable();

      // Filter out the current user
      users = users.Where(u => u.Id != userParams.UserId);

      // Filter out the gender
      users = users.Where(u => u.Gender == userParams.Gender);

      // If the age filter is not default 
      if (userParams.MinAge != 18 | userParams.MaxAge != 99)
      {
        // We store the 
        // Minimum date of birth will be today minus the max age the person selected
        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        // Maximum date of birth is going to be today minus the min age the person selcted.
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
        // Add filter to the users QUERYABLE.
        users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

      }

      // Check if we want to get a list of users that like the current logged in user
      if (userParams.Likers)
      {
        var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
        users = users.Where(u => userLikers.Contains(u.Id));
      }

      // Check if we want to get a like of users that are liked by the currently logged in user.
      if (userParams.Likees)
      {
        var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
        users = users.Where(u => userLikees.Contains(u.Id));
      }

      // Check if order By is passed in to Params
      if (!string.IsNullOrEmpty(userParams.OrderBy))
      {
        switch (userParams.OrderBy) 
        {
          case "created":
            users = users.OrderByDescending(u => u.Created);
            break;
          default:
            users = users.OrderByDescending(u => u.LastActive);
            break;
        }
      }
      
      // Pass in our source and excecute the method.
			return await PageList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
    }

    private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
    {
      // Get the user and include the list of Likers and Likees.
      var user = await _context.Users
        .Include(x => x.Likers)
        .Include(x => x.Likees)
        .FirstOrDefaultAsync(u => u.Id == id);
      
      // Check if we want to return a list of all the users that have liked the currently logged in user.
      if (likers) 
      {
        // Get a list of all users where this user is the Likee. So return a list of all the Likers.
        return user.Likers.Where(u => u.LikeeId == id).Select(u => u.LikerId);
      }
      else 
      {
        // Get a list of user where this user is the Liker, so return a list of all the users that this user likes.
        return user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
      }

    }

    public async Task<Photo> GetPhoto(int id) 
    {
      return await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> SaveAll()
    {
			// If SaveChagnes returns more than 0 SaveAll will return True otherwise it will return false.
      return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Photo> GetMainPhotoForUser(int userId)
    {
      return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
    }

    public async Task<Like> GetLike(int userId, int recipientId)
    {
      // Make sure realtionship between liker and likee exists.
      return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<PageList<Message>> GetMessagesForUser(MessageParams messageParams)
    {
      var messages = _context.Messages
        // We want to inlcue the sender in this case
        .Include(u => u.Sender)
        // We also want to include the photos the sender will have becuase we will be displaying them in the messages
        .ThenInclude(p => p.Photos)
        // Also inlcude the recipient and their photos.
        .Include(u => u.Recipient)
        .ThenInclude(p => p.Photos)
        // Change this to a Queryable becuase we will be adding a Where clause
        .AsQueryable();

      // Filter out
      switch(messageParams.MessageContainer) 
      {
        case "Inbox":
          messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.RecipientDeleted == false);
          break;
        case "Outbox":
          messages = messages.Where(m => m.SenderId == messageParams.UserId && m.SenderDeleted == false);
          break;
        default:
          // Default will be all unread messages for the user.
          messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.IsRead == false && u.RecipientDeleted == false);
          break;
      }

      messages = messages.OrderByDescending(d => d.MessageSent);

      return await PageList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
    {
      var messages = await _context.Messages
        // We want to inlcue the sender in this case
        .Include(u => u.Sender)
        // We also want to include the photos the sender will have becuase we will be displaying them in the messages
        .ThenInclude(p => p.Photos)
        // Also inlcude the recipient and their photos.
        .Include(u => u.Recipient)
        .ThenInclude(p => p.Photos)
        // Get the complete thread. Any messagees sent sent from or to recipient and vise versa.
        .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false)
        // Order by Mesage sent
        .OrderByDescending(m => m.MessageSent)
        .ToListAsync();

        return messages;
    }
  }
}