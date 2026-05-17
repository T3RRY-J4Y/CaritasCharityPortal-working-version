using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Services;
using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Controllers
{
    public class DonorController : Controller
    {
        private readonly IDonationService _donationService;

        public DonorController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        public IActionResult Donate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDonation([FromBody] DonationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var donation = await _donationService.CreateDonationAsync(dto);
                return Ok(new { success = true, message = "Thank you for your donation!", reference = donation.Reference });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}