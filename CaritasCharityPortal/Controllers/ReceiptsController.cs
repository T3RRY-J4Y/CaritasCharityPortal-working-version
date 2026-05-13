using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReceiptsController : Controller
    {
        private readonly IReceiptService _receiptService;
        private readonly IWebHostEnvironment _env;

        public ReceiptsController(IReceiptService receiptService, IWebHostEnvironment env)
        {
            _receiptService = receiptService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();
            return View(receipts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile image, string Name, string Category, string PurchaseDate)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "Please select an image file." });

            if (string.IsNullOrWhiteSpace(Name))
                return BadRequest(new { success = false, message = "Receipt name is required." });

            if (string.IsNullOrWhiteSpace(Category))
                return BadRequest(new { success = false, message = "Category is required." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
                return BadRequest(new { success = false, message = "Only JPG, PNG and GIF images are allowed." });

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new { success = false, message = "Image must be under 5MB." });

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "receipts");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(image.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var fileLink = $"/uploads/receipts/{fileName}";

                var dto = new ReceiptCreateDto
                {
                    Name = Name,
                    Category = Category,
                    FileLink = fileLink,
                    PurchaseDate = DateTime.TryParse(PurchaseDate, out var pd) ? pd : DateTime.Today
                };

                var receipt = await _receiptService.CreateReceiptAsync(dto);
                return Ok(new { success = true, receipt });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, string? fileLink)
        {
            if (!string.IsNullOrEmpty(fileLink) && fileLink.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_env.WebRootPath, fileLink.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            var result = await _receiptService.DeleteReceiptAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}