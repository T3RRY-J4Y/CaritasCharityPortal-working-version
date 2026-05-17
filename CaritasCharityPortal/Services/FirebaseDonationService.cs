using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.RegularExpressions;

namespace CaritasCharityPortal.Services
{
    public class FirebaseDonationService : IDonationService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string DonationsNode = "donations";

        public FirebaseDonationService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl);
        }

        public async Task<List<Donation>> GetAllDonationsAsync()
        {
            var donations = await _firebaseClient
                .Child(DonationsNode)
                .OrderBy("donationDate")
                .OnceAsync<Donation>();

            return donations.Select(item =>
            {
                item.Object.Id = item.Key;
                return item.Object;
            }).OrderByDescending(d => d.DonationDate).ToList();
        }

        public async Task<List<Donation>> GetDonationsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new List<Donation>();
            var all = await GetAllDonationsAsync();
            return all.Where(d => d.Email == email).ToList();
        }

        public async Task<Donation?> GetDonationByIdAsync(string id)
        {
            var donation = await _firebaseClient
                .Child(DonationsNode)
                .Child(id)
                .OnceSingleAsync<Donation>();
            if (donation != null) donation.Id = id;
            return donation;
        }

        public async Task<Donation> CreateDonationAsync(DonationCreateDto dto)
        {
            var donation = new Donation
            {
                Reference = GenerateReference(),
                DonorName = dto.DonorName,
                DonationType = dto.DonationType,
                DonationDate = dto.DonationDate,
                Description = dto.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Email = dto.Email
            };

            if (dto.DonationType == "Money")
                donation.Amount = ParseMonetaryAmount(dto.QuantityOrAmount ?? "0");
            else
                donation.Quantity = ParseQuantity(dto.QuantityOrAmount ?? "0");

            var result = await _firebaseClient
                .Child(DonationsNode)
                .PostAsync(donation);

            donation.Id = result.Key;
            return donation;
        }

        public async Task<bool> UpdateDonationStatusAsync(string id, string status)
        {
            var donation = await GetDonationByIdAsync(id);
            if (donation == null) return false;
            donation.Status = status;
            await _firebaseClient.Child(DonationsNode).Child(id).PatchAsync(donation);
            return true;
        }

        public async Task<bool> DeleteDonationAsync(string id)
        {
            await _firebaseClient.Child(DonationsNode).Child(id).DeleteAsync();
            return true;
        }

        public async Task<DonationStatsDto> GetStatsAsync()
        {
            var donations = await GetAllDonationsAsync();
            return new DonationStatsDto
            {
                TotalDonations = donations.Count,
                TotalMonetary = donations.Where(d => d.DonationType == "Money" && d.Amount.HasValue).Sum(d => d.Amount ?? 0),
                TotalFoodParcels = donations.Where(d => d.DonationType == "Food Parcels" && d.Quantity.HasValue).Sum(d => d.Quantity ?? 0),
                PendingReview = donations.Count(d => d.Status == "Pending")
            };
        }

        private string GenerateReference() => $"DON-{DateTime.Now:yyyyMMddHHmmssfff}";
        private decimal ParseMonetaryAmount(string input)
        {
            var cleaned = Regex.Replace(input, @"[^0-9.,]", "").Replace(",", ".");
            return decimal.TryParse(cleaned, out var result) ? result : 0;
        }
        private int ParseQuantity(string input)
        {
            var cleaned = Regex.Replace(input, @"[^0-9]", "");
            return int.TryParse(cleaned, out var result) ? result : 0;
        }
    }
}