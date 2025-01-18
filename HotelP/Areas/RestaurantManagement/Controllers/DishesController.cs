using HMS.Entities;
using HMS.Services;
using Hotel.Areas.RestaurantManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hotel.Areas.RestaurantManagement.Controllers
{
    [Area("RestaurantManagement")]
    [Authorize(Roles = "Admin,admin")]

    public class DishesController : Controller
    {
        private readonly DishesService _dishesService;
        private readonly CategoryService _categoryService;

        public DishesController(DishesService dishesService,
                                CategoryService categoryService)
        {
            _dishesService = dishesService;
            _categoryService = categoryService;
        }

        // GET: Lista dań z wyszukiwaniem
        [HttpGet]
        public IActionResult Index(string searchTerm, int? categoryId)
        {
            var categories = _categoryService.GetAllCategories();
            var dishes = string.IsNullOrEmpty(searchTerm) && !categoryId.HasValue
                ? _dishesService.GetAllDishes()
                : _dishesService.SearchDishes(searchTerm, categoryId);

            var model = new DishesListingModel
            {
                Dishes = dishes,
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId,
                Categories = categories
            };

            return View(model);
        }

        // GET: Tworzenie/edycja
        [HttpGet]
        public IActionResult Action(int? id)
        {
            var categories = _categoryService.GetAllCategories();

            var model = new DishActionModel
            {
                ID = 0,
                Name = string.Empty,
                Description = string.Empty,
                Price = 0,
                CategoryID = 0,
                Categories = categories,
                DishPictures = new List<DishPicture>()
            };

            if (id.HasValue && id.Value > 0)
            {
                var dish = _dishesService.GetDishById(id.Value);
                if (dish == null) return NotFound("Dish not found.");

                model.ID = dish.ID;
                model.Name = dish.Name;
                model.Description = dish.Description;
                model.Price = dish.Price;
                model.CategoryID = dish.CategoryID;
                model.DishPictures = dish.DishPictures ?? new List<DishPicture>();
            }

            return PartialView("_Action", model);
        }

        // POST: Zapis tworzenia/edycji
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Action(DishActionModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Nieprawidłowe dane formularza." });
            }

            // Parsujemy PictureIDs
            var pictureIDs = new List<int>();
            if (!string.IsNullOrEmpty(model.PictureIDs))
            {
                pictureIDs = model.PictureIDs
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            }

            bool result;
            try
            {
                if (model.ID > 0)
                {
                    // Edycja
                    var existingDish = _dishesService.GetDishById(model.ID);
                    if (existingDish == null)
                        return Json(new { success = false, message = "Dish not found for edit." });

                    existingDish.Name = model.Name;
                    existingDish.Description = model.Description ?? "";
                    existingDish.Price = model.Price;
                    existingDish.CategoryID = model.CategoryID;

                    result = _dishesService.UpdateDish(existingDish, pictureIDs);
                }
                else
                {
                    // Nowy obiekt
                    var newDish = new Dish
                    {
                        Name = model.Name,
                        Description = model.Description ?? "",
                        Price = model.Price,
                        CategoryID = model.CategoryID
                    };

                    result = _dishesService.SaveDish(newDish, pictureIDs);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Błąd przy zapisie: {ex.Message}" });
            }

            if (result)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Nie udało się zapisać danych." });
        }

        // GET: potwierdzenie usunięcia
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var dish = _dishesService.GetDishById(id);
            if (dish == null)
                return NotFound("Dish not found.");

            var model = new DishActionModel
            {
                ID = dish.ID,
                Name = dish.Name
            };
            return PartialView("_Delete", model);
        }

        // POST: usunięcie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(DishActionModel model)
        {
            if (model == null || model.ID <= 0)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            var dish = _dishesService.GetDishById(model.ID);
            if (dish == null)
                return Json(new { success = false, message = "Dish not found to delete." });

            var result = _dishesService.DeleteDish(dish.ID);
            if (result)
                return Json(new { success = true, message = "Dish deleted successfully." });
            else
                return Json(new { success = false, message = "Error while deleting dish." });
        }
    }
}
