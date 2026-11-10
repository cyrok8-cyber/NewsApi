using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;

namespace NewsApi.Services
{
    public interface IEditorialOfficeService
    {
        Task<List<EditorialOfficeResponse>> GetAllAsync();
        Task<EditorialOfficeResponse> GetByIdAsync(int id);
        Task<EditorialOfficeResponse> CreateAsync(CreateEditorialOfficeRequest request);
        Task UpdateAsync(int id, UpdateEditorialOfficeRequest request);
        Task DeleteAsync(int id);
        Task AddMemberAsync(int officeId, int userId);
        Task RemoveMemberAsync(int officeId, int userId);
    }
}
