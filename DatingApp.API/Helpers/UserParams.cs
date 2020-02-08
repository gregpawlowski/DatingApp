namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        // Default max page size
        private const int MaxPageSize = 20;
        public int PageNumber { get; set; } = 1;

        // Default pageSize if the user doesn't set it.
        private int _pageSize = 10;
        // Use a custom getter and setter to limit the page size. If requested higher then maxPage size then it defaults to maxPageSize.
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

        public int UserId { get; set; }

        public string Gender { get; set; }

        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public string OrderBy { get; set; }

        public bool Likees { get; set; } = false;
        public bool Likers { get; set; } = false;
        
    }
}