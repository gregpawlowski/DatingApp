using System;

namespace DatingApp.API.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int RecipientId { get; set; }
        public User Recipient { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        // DateTime is option because we don't want a default value when it's sent initially
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
        // We only want to delete the message if both sender and reciever delted the message.
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }

    }
}