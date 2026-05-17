using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly ICommunityService _communityService;

        public CommunityController(ICommunityService communityService)
        {
            _communityService = communityService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _communityService.GetCommunityDataAsync();
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommunityPostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data." });

            var userEmail = User.Identity?.Name ?? "anonymous";
            try
            {
                var post = await _communityService.CreatePostAsync(dto, userEmail);
                return Ok(new { success = true, message = "Post created successfully.", id = post.Id });
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
            var result = await _communityService.UpdatePostStatusAsync(id, status);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _communityService.DeletePostAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> OfferHelp(string id)
        {
            var result = await _communityService.IncrementResponseCountAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true, message = "Thank you for offering help!" });
        }
    }
}