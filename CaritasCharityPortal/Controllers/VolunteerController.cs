using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly IVolunteerService _volunteerService;

        public VolunteerController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            List<Volunteer> volunteers;
            bool hasApplied = false;

            if (isAdmin)
            {
                volunteers = await _volunteerService.GetAllVolunteersAsync();
            }
            else
            {
                var userEmail = User.Identity?.Name;
                var all = await _volunteerService.GetAllVolunteersAsync();
                volunteers = all.Where(v => v.Email == userEmail).ToList();
                hasApplied = volunteers.Any();
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.HasApplied = hasApplied;
            return View(volunteers);
        }

        [AllowAnonymous]
        public IActionResult Apply() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] VolunteerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Phone) ||
                string.IsNullOrWhiteSpace(dto.Location) ||
                string.IsNullOrWhiteSpace(dto.Skills) ||
                string.IsNullOrWhiteSpace(dto.Availability) ||
                string.IsNullOrWhiteSpace(dto.Motivation))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            var existing = await _volunteerService.GetVolunteerByEmailAsync(dto.Email);
            if (existing != null && existing.Status == "Active")
            {
                return BadRequest(new { success = false, message = "This email is already registered as an active volunteer." });
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var volunteer = await _volunteerService.CreateVolunteerAsync(dto);
                return Ok(new { success = true, message = "Application submitted successfully!", id = volunteer.Id });
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
            var result = await _volunteerService.UpdateVolunteerStatusAsync(id, status);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _volunteerService.DeleteVolunteerAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddHours(string id, int hours)
        {
            var result = await _volunteerService.AddHoursAsync(id, hours);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}