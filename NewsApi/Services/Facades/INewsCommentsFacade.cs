using NewsApi.DTOs.Responses;

namespace NewsApi.Services.Facades
{
    // Фасад нужен, когда сценарий требует сразу несколько сервисов -
    // сами сервисы друг друга не вызывают, это делает фасад
    public interface INewsCommentsFacade
    {
        Task<NewsWithCommentsResponse> GetNewsWithCommentsAsync(int newsId);
    }
}