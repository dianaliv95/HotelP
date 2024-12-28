using HMS.Entities;
using HMS.Services;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class AccommodationTypesController : Controller
    {
        private readonly AccommodationTypesService _AccommodationTypesService;


        public AccommodationTypesController(AccommodationTypesService AccommodationTypesService)
        {
            _AccommodationTypesService = AccommodationTypesService;
        }
        
        public IActionResult Index(string searchterm)
        {
            List<AccommodationType> AccommodationTypes;
            if (!string.IsNullOrEmpty(searchterm))
            {
                AccommodationTypes = _AccommodationTypesService.SearchAccommodationTypes(searchterm);
            }
            else
            {
                AccommodationTypes = _AccommodationTypesService.GetAllAccommodationTypes();
            }

            var model = new AccommodationTypesListingModel
            {
                AccommodationTypes = AccommodationTypes,
                SearchTerm = searchterm

            };

            return View(model);
        }




        [HttpGet]
        public IActionResult Action(int? ID)
        {

            var model = new AccommodationTypesActionModel
            {
                Name = string.Empty,
                Description = string.Empty
            };
            if (ID.HasValue)
            {
                var AccommodationTypes = _AccommodationTypesService.GetAccommodationTypeByID(ID.Value);
                model.ID = AccommodationTypes.ID;
                model.Name = AccommodationTypes.Name;
                model.Description = AccommodationTypes.Description;
            }

            return PartialView("_Action", model);
        }
        [HttpPost]
        public JsonResult Action(AccommodationTypesActionModel model)
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
                    var AccommodationTypes = _AccommodationTypesService.GetAccommodationTypeByID(model.ID);
                    if (AccommodationTypes == null)
                    {
                        return Json(new { success = false, message = "Nie znaleziono rekordu do edycji." });
                    }
                    AccommodationTypes.Name = model.Name;
                    AccommodationTypes.Description = model.Description ?? "";
                    result = _AccommodationTypesService.UpdateAccommodationType(AccommodationTypes);
                }
                else
                {
                    var AccommodationType = new AccommodationType
                    {
                        Name = model.Name,
                        Description = model.Description ?? ""
                    };

                    result = _AccommodationTypesService.SaveAccommodationType(AccommodationType);
                }
            }
            catch (Exception ex)
            {
                // Logowanie szczegółów błędu
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


        [HttpGet]
        public IActionResult Delete(int ID)
        {
            // Pobierz rekord z bazy danych na podstawie ID
            var AccommodationType = _AccommodationTypesService.GetAccommodationTypeByID(ID);

            // Jeśli rekord nie istnieje, zwróć komunikat o błędzie
            if (AccommodationType == null)
            {
                return NotFound("Nie znaleziono typu zakwaterowania o podanym ID.");
            }

            // Przygotuj model do widoku
            var model = new AccommodationTypesActionModel
            {
                ID = AccommodationType.ID,
                Name = AccommodationType.Name,
                Description = AccommodationType.Description
            };

            // Zwróć widok w formie częściowego widoku (partial view)
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public JsonResult Delete(AccommodationTypesActionModel model)
        {
            if (model == null || model.ID <= 0)
            {
                return Json(new { success = false, message = "Nieprawidłowe dane wejściowe." });
            }

            var AccommodationType = _AccommodationTypesService.GetAccommodationTypeByID(model.ID);

            if (AccommodationType == null)
            {
                return Json(new { success = false, message = "Nie znaleziono typu zakwaterowania o podanym ID." });
            }

            var result = _AccommodationTypesService.DeleteAccommodationType(AccommodationType.ID);

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











