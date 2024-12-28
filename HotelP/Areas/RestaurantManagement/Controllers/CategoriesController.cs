using HMS.Entities;
using HMS.Services;
using Hotel.Areas.RestaurantManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hotel.Areas.RestaurantManagement.Controllers
{
    [Area("RestaurantManagement")]
    public class CategoriesController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(CategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public IActionResult Index(string searchTerm)
        {
            List<Category> categories;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                categories = _categoryService.SearchCategory(searchTerm);
            }
            else
            {
                categories = _categoryService.GetAllCategories();
            }

            var model = new CategoriesListingModel
            {
                Categories = categories,
                SearchTerm = searchTerm
            };

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Action(int? ID)
        {
            _logger.LogInformation("Action GET called with ID: {ID}", ID);
            var model = new CategoryActionModel
            {
                Name = string.Empty,
                Description = string.Empty
            };

            if (ID.HasValue)
            {
                var category = _categoryService.GetCategoryById(ID.Value);

                if (category == null)
                {
                    _logger.LogWarning("Category with ID {ID} not found.", ID.Value);
                    return NotFound("Nie znaleziono kategorii o podanym ID.");
                }

                model.ID = category.ID;
                model.Name = category.Name;
                model.Description = category.Description;
            }

            return PartialView("_Action", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public JsonResult Action(CategoryActionModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Model jest nullem." });
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                return Json(new { success = false, message = "Pole 'Name' jest wymagane." });
            }

            var result = false;

            try
            {
                if (model.ID > 0)
                {
                    var category = _categoryService.GetCategoryById(model.ID);

                    if (category == null)
                    {
                        return Json(new { success = false, message = "Nie znaleziono rekordu do edycji." });
                    }

                    category.Name = model.Name;
                    category.Description = model.Description ?? string.Empty;

                    result = _categoryService.UpdateCategory(category);
                }
                else
                {
                    var newCategory = new Category
                    {
                        Name = model.Name,
                        Description = model.Description ?? string.Empty
                    };

                    result = _categoryService.SaveCategory(newCategory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving category.");
                return Json(new { success = false, message = "Wystąpił błąd podczas zapisywania danych." });
            }

            return Json(new { success = result });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Delete(int ID)
        {
            var category = _categoryService.GetCategoryById(ID);

            if (category == null)
            {
                return NotFound("Nie znaleziono kategorii o podanym ID.");
            }

            return PartialView("_Delete", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public JsonResult Delete(int ID, string name)
        {
            if (ID <= 0)
            {
                return Json(new { success = false, message = "Nieprawidłowe ID." });
            }

            var category = _categoryService.GetCategoryById(ID);

            if (category == null)
            {
                return Json(new { success = false, message = "Nie znaleziono kategorii do usunięcia." });
            }

            var result = _categoryService.DeleteCategory(ID);

            if (result)
            {
                return Json(new { success = true, message = $"Kategoria '{category.Name}' została usunięta." });
            }
            else
            {
                return Json(new { success = false, message = "Błąd podczas usuwania kategorii." });
            }
        }
    }
}
