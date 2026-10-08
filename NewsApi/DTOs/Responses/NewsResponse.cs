namespace NewsApi.DTOs.Responses
{
    public class NewsResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public UserShortResponse InsertedBy { get; set; } = null!;
        public UserShortResponse? UpdatedBy { get; set; }
    }
}