using HMS.Entities;
using HMS.ViewModels;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin,admin")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserService userService, RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string roleID, int page = 1, int recordSize = 10)
        {
            var users = await _userService.GetUsersAsync(searchTerm, roleID, page, recordSize);
            var totalRecords = await _userService.GetUsersCountAsync(searchTerm, roleID);

            var model = new UsersListingModel
            {
                Users = users,
                SearchTerm = searchTerm,
                RoleID = roleID,
                Roles = await _roleManager.Roles.Select(r => new RoleDTO
                {
                    ID = r.Id,
                    Name = r.Name
                }).ToListAsync(),
                Pager = new Pager(totalRecords, page, recordSize)
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Action(string id)
        {
            ModelState.Clear();

            var model = new UserActionModel
            {
                AvailableRoles = await _roleManager.Roles.Select(r => new RoleDTO
                {
                    ID = r.Id,
                    Name = r.Name
                }).ToListAsync()
            };

            if (!string.IsNullOrEmpty(id))
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null) return NotFound();

                model.ID = user.ID;
                model.FullName = user.FullName ?? string.Empty;
                model.Email = user.Email;
                model.Username = user.Username;
                model.Country = user.Country ?? string.Empty;
                model.City = user.City ?? string.Empty;
                model.Address = user.Address ?? string.Empty;
                model.Role = user.Role ?? string.Empty;
            }

            return PartialView("_Action", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Action(UserActionModel model)
        {
            if (string.IsNullOrEmpty(model.ID)) 
            {
                ModelState.Remove(nameof(UserActionModel.ID));
            }
            if (string.IsNullOrEmpty(model.City))
            {
                ModelState.Remove(nameof(model.City));
            }
            if (string.IsNullOrEmpty(model.Role))
            {
                return Json(new { success = false, message = "Role is required." });
            }
            if (string.IsNullOrEmpty(model.Address))
            {
                ModelState.Remove(nameof(model.Address));
            }

            if (string.IsNullOrEmpty(model.Country))
            {
                ModelState.Remove(nameof(model.Country));
            }
            if (string.IsNullOrEmpty(model.FullName))
            {
                ModelState.Remove(nameof(model.FullName));
            }
            ModelState.Remove(nameof(UserActionModel.AvailableRoles));

           
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Invalid input.", errors });
            }

            var (success, validationErrors) = await _userService.SaveOrUpdateUserAsync(new UserDTO
            {
                ID = model.ID,
                FullName = model.FullName,
                Email = model.Email,
                Username = model.Username,
                Country = model.Country,
                City = model.City,
                Address = model.Address,
                Role = model.Role
            });

            if (success)
            {
                return Json(new { success });
            }
            else
            {
                var errorMessage = string.Join("; ", validationErrors);
                return Json(new { success = false, message = errorMessage });
            }
        }






        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var model = new UserActionModel
            {
                ID = user.ID,
                Email = user.Email,
                Username = user.Username
            };

            return PartialView("_Delete", model);
        }


        [HttpPost]
        public async Task<JsonResult> Delete(UserActionModel model)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(model.ID);
                return Json(new { success, message = success ? "User deleted successfully." : "Error deleting user." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred.", details = ex.Message });
            }
        }

    }
}
