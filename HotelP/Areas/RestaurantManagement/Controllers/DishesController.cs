using HMS.Entities;
using HMS.Services;
using Hotel.Areas.RestaurantManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.RestaurantManagement.Controllers
{
    [Area("RestaurantManagement")]
    public class DishesController : Controller
    {
        private readonly DishesService _dishesService;
        private readonly CategoryService _categoryService;

        public DishesController(DishesService dishesService, CategoryService categoryService)
        {
            _dishesService = dishesService;
            _categoryService = categoryService;
        }

        public IActionResult Index(string searchTerm, int? categoryId)
        {
            // Pobranie listy dań na podstawie filtrów
            var dishes = categoryId.HasValue
                ? _dishesService.GetDishesByCategory(categoryId.Value)
                : _dishesService.SearchDishes(searchTerm);

            // Przygotowanie ViewModel-u
            var model = new DishesListingModel
            {
                Dishes = dishes,
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId,
                Categories = _categoryService.GetAllCategories() // Pobranie unikalnych kategorii
            };

            return View(model);
        }

        // Akcja GET dla formularza dodawania/edycji
        [HttpGet]
        public IActionResult Action(int? ID)
        {
            var categories = _categoryService.GetAllCategories(); // Pobierz kategorie z bazy danych

            var model = new DishActionModel
            {
                ID = ID ?? 0,
                Name = string.Empty,
                Description = string.Empty,
                Price = 0,
                CategoryID = 0,
                Categories = categories
            };

            if (ID.HasValue)
            {
                var dish = _dishesService.GetDishById(ID.Value);
                if (dish != null)
                {
                    model.ID = dish.ID;
                    model.Name = dish.Name;
                    model.Description = dish.Description;
                    model.Price = dish.Price;
                    model.CategoryID = dish.CategoryID;
                }
            }

            return PartialView("_Action", model);
        }


        // Akcja POST dla zapisywania danych
        [HttpPost]
        public JsonResult Action(DishActionModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Model jest nullem." });
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                return Json(new { success = false, message = "Pole 'Name' jest wymagane." });
            }

            if (model.CategoryID <= 0)
            {
                return Json(new { success = false, message = "Wybór kategorii jest wymagany." });
            }

            var result = false;

            try
            {
                if (model.ID > 0)
                {
                    var dish = _dishesService.GetDishById(model.ID);
                    if (dish == null)
                    {
                        return Json(new { success = false, message = "Nie znaleziono rekordu do edycji." });
                    }

                    dish.Name = model.Name;
                    dish.Description = model.Description ?? "";
                    dish.Price = model.Price;
                    dish.CategoryID = model.CategoryID;

                    result = _dishesService.UpdateDish(dish);
                }
                else
                {
                    var newDish = new Dish
                    {
                        Name = model.Name,
                        Description = model.Description ?? "",
                        Price = model.Price,
                        CategoryID = model.CategoryID
                    };

                    result = _dishesService.SaveDish(newDish);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Wystąpił błąd podczas zapisywania danych." });
            }

            if (result)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Błąd podczas zapisywania danych." });
            }
        }

        // Akcja GET dla usuwania
        [HttpGet]
        public IActionResult Delete(int ID)
        {
            var dish = _dishesService.GetDishById(ID);

            if (dish == null)
            {
                return NotFound("Nie znaleziono dania o podanym ID.");
            }

            var model = new DishActionModel
            {
                ID = dish.ID,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                CategoryID = dish.CategoryID
            };

            return PartialView("_Delete", model);
        }

        // Akcja POST dla usuwania
        [HttpPost]
        public JsonResult Delete(DishActionModel model)
        {
            if (model == null || model.ID <= 0)
            {
                return Json(new { success = false, message = "Nieprawidłowe dane wejściowe." });
            }

            var dish = _dishesService.GetDishById(model.ID);
            if (dish == null)
            {
                return Json(new { success = false, message = "Nie znaleziono dania o podanym ID." });
            }

            var result = _dishesService.DeleteDish(dish.ID);

            if (result)
            {
                return Json(new { success = true, message = "Rekord został usunięty." });
            }
            else
            {
                return Json(new { success = false, message = "Błąd podczas usuwania rekordu." });
            }
        }
    }
}
