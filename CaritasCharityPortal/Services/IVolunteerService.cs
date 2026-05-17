using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IVolunteerService
    {
        Task<List<Volunteer>> GetAllVolunteersAsync();
        Task<Volunteer?> GetVolunteerByIdAsync(string id);
        Task<Volunteer?> GetVolunteerByEmailAsync(string email);
        Task<Volunteer> CreateVolunteerAsync(VolunteerCreateDto dto);
        Task<bool> UpdateVolunteerStatusAsync(string id, string status);
        Task<bool> AddHoursAsync(string id, int hours);
        Task<bool> DeleteVolunteerAsync(string id);
        Task<int> GetPendingCountAsync();
        Task<int> GetActiveCountAsync();
        Task<int> GetTotalHoursAsync();
    }
}