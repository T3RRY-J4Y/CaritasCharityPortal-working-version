using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace CaritasCharityPortal.Services
{
    public class FirebaseAssistanceService : IAssistanceService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string Node = "assistanceRequests";

        public FirebaseAssistanceService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl!);
        }

        public async Task<AssistancePageViewModel> GetPageDataAsync()
        {
            var all = await GetAllRequestsAsync();
            return new AssistancePageViewModel
            {
                Rejected = all.Count(r => r.Status == "Rejected"),
                Pending = all.Count(r => r.Status == "Pending"),
                Approved = all.Count(r => r.Status == "Approved"),
                Urgent = all.Count(r => r.IsUrgent && r.Status == "Pending"),
                Requests = all.OrderByDescending(r => r.SubmittedAt).ToList()
            };
        }

        public async Task<List<AssistanceRequest>> GetAllRequestsAsync()
        {
            var snapshot = await _firebaseClient
                .Child(Node)
                .OrderBy("submittedAt")
                .OnceAsync<AssistanceRequest>();

            return snapshot.Select(item =>
            {
                item.Object.Id = item.Key;
                return item.Object;
            }).ToList();
        }

        public async Task<AssistanceRequest> CreateRequestAsync(AssistanceRequest request)
        {
            if (!request.DeclarationAccepted)
                throw new InvalidOperationException("You must accept the declaration.");

            request.ReferenceNumber = GenerateReference();
            request.SubmittedAt = DateTime.UtcNow;
            request.Status = "Pending";

            var result = await _firebaseClient.Child(Node).PostAsync(request);
            request.Id = result.Key;
            return request;
        }

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            var request = await _firebaseClient.Child(Node).Child(id).OnceSingleAsync<AssistanceRequest>();
            if (request == null) return false;
            request.Status = status;
            await _firebaseClient.Child(Node).Child(id).PatchAsync(request);
            return true;
        }

        public async Task<bool> DeleteRequestAsync(string id)
        {
            await _firebaseClient.Child(Node).Child(id).DeleteAsync();
            return true;
        }

        private string GenerateReference() => $"REQ-{DateTime.Now:yyyyMMddHHmmss}";
    }
}