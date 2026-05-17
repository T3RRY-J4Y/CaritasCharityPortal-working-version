using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace CaritasCharityPortal.Services
{
    public class FirebaseCommunityService : ICommunityService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string Node = "communityPosts";

        public FirebaseCommunityService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl!);
        }

        public async Task<List<CommunityPost>> GetAllPostsAsync()
        {
            var snapshot = await _firebaseClient
                .Child(Node)
                .OrderBy("createdAt")
                .OnceAsync<CommunityPost>();

            return snapshot.Select(item =>
            {
                item.Object.Id = item.Key;
                return item.Object;
            }).OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<CommunityViewModel> GetCommunityDataAsync()
        {
            var posts = await GetAllPostsAsync();
            var totalResponses = posts.Sum(p => p.ResponseCount);
            return new CommunityViewModel
            {
                TotalPosts = posts.Count,
                TotalResponses = totalResponses,
                ActiveRequests = posts.Count(p => p.Status == "Open"),
                UrgentPosts = posts.Count(p => p.IsUrgent && p.Status == "Open"),
                Posts = posts
            };
        }

        public async Task<CommunityPost> CreatePostAsync(CommunityPostDto dto, string userEmail)
        {
            var post = new CommunityPost
            {
                PostId = GeneratePostId(),
                RequestingFor = dto.RequestingFor,
                FamilyCount = dto.FamilyCount,
                AssistanceType = dto.AssistanceType,
                QuantityNeeded = dto.QuantityNeeded,
                Description = dto.Description,
                ContactInfo = dto.ContactInfo,
                IsUrgent = dto.IsUrgent,
                Status = "Open",
                UserEmail = userEmail,
                CreatedAt = DateTime.UtcNow,
                ResponseCount = 0
            };

            var result = await _firebaseClient.Child(Node).PostAsync(post);
            post.Id = result.Key;
            return post;
        }

        public async Task<bool> UpdatePostStatusAsync(string id, string status)
        {
            var post = await _firebaseClient.Child(Node).Child(id).OnceSingleAsync<CommunityPost>();
            if (post == null) return false;
            post.Status = status;
            await _firebaseClient.Child(Node).Child(id).PatchAsync(post);
            return true;
        }

        public async Task<bool> DeletePostAsync(string id)
        {
            await _firebaseClient.Child(Node).Child(id).DeleteAsync();
            return true;
        }

        public async Task<bool> IncrementResponseCountAsync(string id)
        {
            var post = await _firebaseClient.Child(Node).Child(id).OnceSingleAsync<CommunityPost>();
            if (post == null) return false;
            post.ResponseCount++;
            await _firebaseClient.Child(Node).Child(id).PatchAsync(post);
            return true;
        }

        private string GeneratePostId() => $"POST-{DateTime.Now:yyyyMMddHHmmss}";
    }
}