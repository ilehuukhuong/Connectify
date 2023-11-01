namespace API.Helpers
{
    public class LikesParams : PaginationParams
    {
        public string Search { get; set; }
        public int UserId { get; set; }
        public string Predicate { get; set; }
    }
}