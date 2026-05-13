using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace CaritasCharityPortal.Services
{
    public class FirebaseVolunteerService : IVolunteerService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string VolunteersNode = "volunteers";

        public FirebaseVolunteerService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl);
        }

        public async Task<List<Volunteer>> GetAllVolunteersAsync()
        {
            var volunteers = await _firebaseClient
                .Child(VolunteersNode)
                .OrderBy("submittedAt")
                .OnceAsync<Volunteer>();
            return volunteers.Select(item =>
            {
                item.Object.Id = item.Key;
                return item.Object;
            }).OrderByDescending(v => v.SubmittedAt).ToList();
        }

        public async Task<Volunteer?> GetVolunteerByIdAsync(string id)
        {
            var volunteer = await _firebaseClient
                .Child(VolunteersNode)
                .Child(id)
                .OnceSingleAsync<Volunteer>();
            if (volunteer != null) volunteer.Id = id;
            return volunteer;
        }

        public async Task<Volunteer?> GetVolunteerByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var all = await GetAllVolunteersAsync();
            return all.FirstOrDefault(v => string.Equals(v.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Volunteer> CreateVolunteerAsync(VolunteerCreateDto dto)
        {
            var volunteer = new Volunteer
            {
                ApplicationId = GenerateApplicationId(),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Location = dto.Location,
                Skills = dto.Skills,
                Availability = dto.Availability,
                Motivation = dto.Motivation,
                Status = "Pending",
                TotalHours = 0,
                SubmittedAt = DateTime.UtcNow
            };

            var result = await _firebaseClient
                .Child(VolunteersNode)
                .PostAsync(volunteer);
            volunteer.Id = result.Key;
            return volunteer;
        }

        public async Task<bool> UpdateVolunteerStatusAsync(string id, string status)
        {
            var volunteer = await GetVolunteerByIdAsync(id);
            if (volunteer == null) return false;
            volunteer.Status = status;
            await _firebaseClient.Child(VolunteersNode).Child(id).PatchAsync(volunteer);
            return true;
        }

        public async Task<bool> AddHoursAsync(string id, int hours)
        {
            var volunteer = await GetVolunteerByIdAsync(id);
            if (volunteer == null) return false;
            volunteer.TotalHours += hours;
            await _firebaseClient.Child(VolunteersNode).Child(id).PatchAsync(volunteer);
            return true;
        }

        public async Task<bool> DeleteVolunteerAsync(string id)
        {
            await _firebaseClient.Child(VolunteersNode).Child(id).DeleteAsync();
            return true;
        }

        public async Task<int> GetPendingCountAsync()
        {
            var vols = await GetAllVolunteersAsync();
            return vols.Count(v => v.Status == "Pending");
        }

        public async Task<int> GetActiveCountAsync()
        {
            var vols = await GetAllVolunteersAsync();
            return vols.Count(v => v.Status == "Active");
        }

        public async Task<int> GetTotalHoursAsync()
        {
            var vols = await GetAllVolunteersAsync();
            return vols.Sum(v => v.TotalHours);
        }

        private string GenerateApplicationId() => $"APP-{DateTime.Now:yyyyMMddHHmmss}";
    }
}