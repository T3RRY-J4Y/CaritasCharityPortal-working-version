using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IAssistanceService
    {
        Task<AssistancePageViewModel> GetPageDataAsync();
        Task<List<AssistanceRequest>> GetAllRequestsAsync();   
        Task<AssistanceRequest> CreateRequestAsync(AssistanceRequest request);
        Task<bool> UpdateStatusAsync(string id, string status);
        Task<bool> DeleteRequestAsync(string id);
    }
}