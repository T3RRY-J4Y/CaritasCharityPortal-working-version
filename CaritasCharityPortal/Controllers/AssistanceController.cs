using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    [Authorize]
    public class AssistanceController : Controller
    {
        private readonly IAssistanceService _assistanceService;

        public AssistanceController(IAssistanceService assistanceService)
        {
            _assistanceService = assistanceService;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var model = await _assistanceService.GetPageDataAsync();
                ViewBag.IsAdmin = true;
                return View(model);
            }
            else
            {
                var allRequests = await _assistanceService.GetAllRequestsAsync();
                var userRequests = allRequests.Where(r => r.UserEmail == userEmail).ToList();

                var model = new AssistancePageViewModel
                {
                    Rejected = userRequests.Count(r => r.Status == "Rejected"),
                    Pending = userRequests.Count(r => r.Status == "Pending"),
                    Approved = userRequests.Count(r => r.Status == "Approved"),
                    Urgent = userRequests.Count(r => r.IsUrgent && r.Status == "Pending"),
                    Requests = userRequests.OrderByDescending(r => r.SubmittedAt).ToList()
                };
                ViewBag.IsAdmin = false;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssistanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data." });

            request.UserEmail = User.Identity?.Name;

            try
            {
                var created = await _assistanceService.CreateRequestAsync(request);
                return Ok(new { success = true, message = "Request submitted successfully.", id = created.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            var result = await _assistanceService.UpdateStatusAsync(id, status);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _assistanceService.DeleteRequestAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}