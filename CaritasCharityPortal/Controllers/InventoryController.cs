using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _inventoryService.GetInventoryDataAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] InventoryItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Category) || string.IsNullOrWhiteSpace(item.Description) || item.Quantity <= 0)
                return BadRequest(new { success = false, message = "Category, description, and positive quantity are required." });

            try
            {
                var newItem = await _inventoryService.AddStockAsync(item);
                return Ok(new { success = true, message = "Stock added successfully.", item = newItem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStock(string id, int quantity)
        {
            if (string.IsNullOrEmpty(id) || quantity <= 0)
                return BadRequest(new { success = false, message = "Item ID and positive quantity required." });

            try
            {
                var updatedItem = await _inventoryService.RemoveStockAsync(id, quantity);
                return Ok(new { success = true, message = "Stock removed successfully.", item = updatedItem });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _inventoryService.DeleteItemAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}