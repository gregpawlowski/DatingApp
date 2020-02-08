namespace DatingApp.API.Models
{
    public class Like
    {
        // Here we have pairs of relationships with a naviagation property to the User.
        // Database will only store LiekerId and LikeeId
        public int LikerId { get; set; }
        public User Liker { get; set; }
        public int LikeeId { get; set; }
        public User Likee { get; set; }
    }
}