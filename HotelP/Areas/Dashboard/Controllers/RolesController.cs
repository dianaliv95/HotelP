using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using HMS.Entities;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin,admin")]
    public class RolesController : Controller
    {
        private readonly RoleService _roleService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<IdentityRole> roleManager, RoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            List<RoleDTO> roles;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                roles = await _roleService.SearchRolesAsync(searchTerm);
            }
            else
            {
                roles = await _roleService.GetAllRolesAsync();
            }

            var model = new RolesListingModel
            {
                Roles = roles,
                SearchTerm = searchTerm
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Action(string id)
        {
            var model = new RoleActionModel();

            if (!string.IsNullOrEmpty(id)) 
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null) return NotFound();

                model.ID = role.Id;
                model.Name = role.Name;
            }

            return PartialView("_Action", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Action(RoleActionModel model)
        {
            if (string.IsNullOrEmpty(model.ID))
            {
                ModelState.Remove("ID");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                return Json(new { success = false, message = "Invalid data.", errors });
            }

            IdentityResult result;

            if (string.IsNullOrEmpty(model.ID)) 
            {
                var role = new IdentityRole
                {
                    Name = model.Name
                };
                result = await _roleManager.CreateAsync(role);
            }
            else 
            {
                var role = await _roleManager.FindByIdAsync(model.ID);
                if (role == null)
                {
                    return Json(new { success = false, message = "Role not found." });
                }

                role.Name = model.Name;
                result = await _roleManager.UpdateAsync(role);
            }

            return Json(new { success = result.Succeeded, message = result.Succeeded ? "Success" : "Error occurred." });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new DeleteRoleModel
            {
                ID = role.Id
            };

            return PartialView("_Delete", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(DeleteRoleModel model)
        {
            _logger.LogInformation($"Attempting to delete role with ID: {model.ID}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                _logger.LogWarning($"Delete failed: Invalid model state. Errors: {string.Join(", ", errors)}");
                return Json(new { success = false, message = "Invalid data.", errors });
            }

            var role = await _roleManager.FindByIdAsync(model.ID);
            if (role == null)
            {
                _logger.LogWarning($"Delete failed: Role with ID {model.ID} not found.");
                return Json(new { success = false, message = "Role not found." });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role with ID {model.ID} deleted successfully.");
                return Json(new { success = true, message = "Role deleted successfully!" });
            }
            else
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Delete failed for role ID {model.ID}: {errorMessages}");
                return Json(new { success = false, message = errorMessages });
            }
        }
    }
}

