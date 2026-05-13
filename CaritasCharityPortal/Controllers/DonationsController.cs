using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly IDonationService _donationService;

        public DonationsController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;
            List<Donation> donations;

            if (User.IsInRole("Admin"))
            {
                donations = await _donationService.GetAllDonationsAsync();
            }
            else if (!string.IsNullOrEmpty(userEmail))
            {
                donations = await _donationService.GetDonationsByEmailAsync(userEmail);
            }
            else
            {
                donations = new List<Donation>();
            }

            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(donations);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DonationCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DonorName) ||
                string.IsNullOrWhiteSpace(dto.DonationType) ||
                string.IsNullOrWhiteSpace(dto.QuantityOrAmount))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            dto.Email = User.Identity?.Name;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var donation = await _donationService.CreateDonationAsync(dto);
                return Ok(new { success = true, donation });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            var result = await _donationService.UpdateDonationStatusAsync(id, status);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _donationService.DeleteDonationAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}