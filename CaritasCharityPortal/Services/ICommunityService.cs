using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface ICommunityService
    {
        Task<CommunityViewModel> GetCommunityDataAsync();
        Task<List<CommunityPost>> GetAllPostsAsync();
        Task<CommunityPost> CreatePostAsync(CommunityPostDto dto, string userEmail);
        Task<bool> UpdatePostStatusAsync(string id, string status);
        Task<bool> DeletePostAsync(string id);
        Task<bool> IncrementResponseCountAsync(string id);
    }
}