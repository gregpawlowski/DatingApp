using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
  public interface IDatingRepository
  {
    void Add<T>(T entity) where T : class;

    void Delete<T>(T entity) where T : class;

    Task<bool> SaveAll();

    Task<PageList<User>> GetUsers(UserParams userParams);

    Task<User> GetUser(int id, bool isCurrentUser);

    Task<Photo> GetPhoto(int id);

    Task<Photo> GetMainPhotoForUser(int userId);
    Task<Like> GetLike(int userId, int recipientId);
    // Get a single message from the database
    Task<Message> GetMessage(int id);
    // Get all messages for a user
    // TODO: Add parameters for Inbox, outbox, unread messages
    Task<PageList<Message>> GetMessagesForUser(MessageParams messageParams);
    // Get a thread between two users
    Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
  }
}