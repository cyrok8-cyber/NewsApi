using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;

namespace NewsApi.Services
{
    public interface IChatService
    {
        Task<PagedResponse<ChatMessageResponse>> GetMessagesAsync(
            int officeId, int userId, string role, int page, int pageSize);

        Task<ChatMessageResponse> SendMessageAsync(
            int officeId, int userId, string role, SendChatMessageRequest request);
    }
}
