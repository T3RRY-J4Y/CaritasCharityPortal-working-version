using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IReceiptService
    {
        Task<List<Receipt>> GetAllReceiptsAsync();
        Task<Receipt?> GetReceiptByIdAsync(string id);
        Task<Receipt> CreateReceiptAsync(ReceiptCreateDto dto);
        Task<bool> DeleteReceiptAsync(string id);
    }
}