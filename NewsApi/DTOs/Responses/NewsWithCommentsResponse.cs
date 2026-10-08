namespace NewsApi.DTOs.Responses
{
    public class NewsWithCommentsResponse
    {
        public NewsResponse News { get; set; } = null!;
        public List<CommentResponse> Comments { get; set; } = new();
    }
}