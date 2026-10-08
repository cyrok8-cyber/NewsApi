namespace NewsApi.DTOs.Responses
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int NewsId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public UserShortResponse InsertedBy { get; set; } = null!;
    }
}