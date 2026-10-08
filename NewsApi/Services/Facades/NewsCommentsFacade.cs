using NewsApi.DTOs.Responses;

namespace NewsApi.Services.Facades
{
    public class NewsCommentsFacade : INewsCommentsFacade
    {
        private readonly INewsService _newsService;
        private readonly ICommentService _commentService;

        public NewsCommentsFacade(INewsService newsService, ICommentService commentService)
        {
            _newsService = newsService;
            _commentService = commentService;
        }

        public async Task<NewsWithCommentsResponse> GetNewsWithCommentsAsync(int newsId)
        {
            // Если новости нет - NewsService сам бросит NotFoundException, фасад её не ловит
            var news = await _newsService.GetByIdAsync(newsId);
            var comments = await _commentService.GetByNewsIdAsync(newsId);

            return new NewsWithCommentsResponse
            {
                News = news,
                Comments = comments
            };
        }
    }
}