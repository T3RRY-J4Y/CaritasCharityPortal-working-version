using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace CaritasCharityPortal.Services
{
    public class FirebaseReceiptService : IReceiptService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string ReceiptsNode = "receipts";

        public FirebaseReceiptService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl);
        }

        public async Task<List<Receipt>> GetAllReceiptsAsync()
        {
            var receipts = await _firebaseClient
                .Child(ReceiptsNode)
                .OrderBy("uploadedAt")
                .OnceAsync<Receipt>();
            return receipts.Select(item =>
            {
                item.Object.Id = item.Key;
                return item.Object;
            }).OrderByDescending(r => r.UploadedAt).ToList();
        }

        public async Task<Receipt?> GetReceiptByIdAsync(string id)
        {
            var receipt = await _firebaseClient
                .Child(ReceiptsNode)
                .Child(id)
                .OnceSingleAsync<Receipt>();
            if (receipt != null) receipt.Id = id;
            return receipt;
        }

        public async Task<Receipt> CreateReceiptAsync(ReceiptCreateDto dto)
        {
            var receipt = new Receipt
            {
                ReceiptId = GenerateReceiptId(),
                Name = dto.Name,
                Category = dto.Category,
                FileLink = dto.FileLink,
                PurchaseDate = dto.PurchaseDate,
                Status = "Uploaded",
                UploadedAt = DateTime.UtcNow
            };
            var result = await _firebaseClient.Child(ReceiptsNode).PostAsync(receipt);
            receipt.Id = result.Key;
            return receipt;
        }

        public async Task<bool> DeleteReceiptAsync(string id)
        {
            await _firebaseClient.Child(ReceiptsNode).Child(id).DeleteAsync();
            return true;
        }

        private string GenerateReceiptId() => $"REC-{DateTime.Now:yyyyMMddHHmmss}";
    }
}