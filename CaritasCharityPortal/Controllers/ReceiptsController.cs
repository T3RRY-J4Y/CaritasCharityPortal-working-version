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

        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        public async Task<IActionResult> Index()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();
            return View(receipts);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReceiptCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Category) ||
                string.IsNullOrWhiteSpace(dto.FileLink))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            if (!Uri.IsWellFormedUriString(dto.FileLink, UriKind.Absolute))
                return BadRequest(new { success = false, message = "Please enter a valid URL." });

            try
            {
                var receipt = await _receiptService.CreateReceiptAsync(dto);
                return Ok(new { success = true, receipt });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _receiptService.DeleteReceiptAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}