using HMS.Entities;
using HMS.Services;
using HMS.ViewModels;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class AccommodationPackagesController : Controller
    {
        private readonly AccommodationPackagesService _AccommodationPackagesService;
        private readonly AccommodationTypesService _AccommodationTypesService;

        public AccommodationPackagesController(AccommodationPackagesService accommodationPackagesService, AccommodationTypesService accommodationTypesService)
        {
            _AccommodationPackagesService = accommodationPackagesService;
            _AccommodationTypesService = accommodationTypesService;
        }
        [HttpGet]
        public IActionResult Index(string searchterm, int? AccommodationTypeID, int page = 1, int recordSize = 10)
        {
            // Pobierz listę typów zakwaterowania
            var accommodationTypes = _AccommodationTypesService.GetAllAccommodationTypes()
                .Select(a => new AccommodationTypeViewModel
                {
                    ID = a.ID,
                    Name = a.Name
                }).ToList();

            // Pobierz pakiety zakwaterowania i mapuj je na widok
            var accommodationPackages = _AccommodationPackagesService.SearchAccommodationPackages(searchterm, AccommodationTypeID, page, recordSize)
    .Select(p => new AccommodationPackageViewModel
    {
        ID = p.ID,
        Name = p.Name,
        FeePerNight = p.FeePerNight,
        AccommodationTypeID = p.AccommodationTypeID,
        AccommodationTypeName = p.AccommodationType != null ? p.AccommodationType.Name : "N/A" // Domyślnie "N/A"
    }).ToList();


            // Przygotuj model paginacji
            int totalItems = accommodationPackages.Count();

            var pager = new Pager(totalItems, page, recordSize);

            // Wypełnij model widoku
            var model = new AccommodationPackagesListingModel
            {
                SearchTerm = searchterm,
                AccommodationTypeID = AccommodationTypeID,
                AccommodationTypes = accommodationTypes,
                AccommodationPackages = accommodationPackages,
                Pager = pager
            };

            return View(model);
        }




        [HttpGet]
        public IActionResult Action(int? ID)
        {
            var model = new AccommodationPackageActionModel();

            // Pobranie listy typów zakwaterowania
            var accommodationTypes = _AccommodationTypesService.GetAllAccommodationTypes();
            model.AccommodationTypes = accommodationTypes.Select(a => new AccommodationTypeViewModel
            {
                ID = a.ID,
                Name = a.Name
            }).ToList();

            if (ID.HasValue)
            {
                var accommodationPackage = _AccommodationPackagesService.GetAccommodationPackageByID(ID.Value);
                if (accommodationPackage != null)
                {
                    model.ID = accommodationPackage.ID;
                    model.AccommodationTypeID = accommodationPackage.AccommodationTypeID;
                    model.Name = accommodationPackage.Name;
                    model.NoOfRoom = accommodationPackage.NoofRoom;
                    model.FeePerNight = accommodationPackage.FeePerNight;
                }
            }

            return PartialView("_Action", model);
        }



        [HttpPost]
        public JsonResult Action(AccommodationPackageActionModel model)
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
                    var AccommodationPackage = _AccommodationPackagesService.GetAccommodationPackageByID(model.ID);
                    if (AccommodationPackage == null)
                    {
                        return Json(new { success = false, message = "Nie znaleziono rekordu do edycji." });
                    }
                    AccommodationPackage.AccommodationTypeID = model.AccommodationTypeID;
                    AccommodationPackage.Name = model.Name;
                    AccommodationPackage.NoofRoom = model.NoOfRoom;
                    AccommodationPackage.FeePerNight = model.FeePerNight;
                    result = _AccommodationPackagesService.UpdateAccommodationPackage(AccommodationPackage);
                }
                else
                {
                    var AccommodationPackages = new AccommodationPackage
                    {
                        AccommodationTypeID = model.AccommodationTypeID,
                        Name = model.Name,
                        NoofRoom = model.NoOfRoom,
                        FeePerNight = model.FeePerNight
                    };

                    result = _AccommodationPackagesService.SaveAccommodationPackage(AccommodationPackages);
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
            var AccommodationPackage = _AccommodationPackagesService.GetAccommodationPackageByID(ID);

            // Jeśli rekord nie istnieje, zwróć komunikat o błędzie
            if (AccommodationPackage == null)
            {
                return NotFound("Nie znaleziono typu zakwaterowania o podanym ID.");
            }

            // Przygotuj model do widoku
            var model = new AccommodationPackageActionModel
            {
                ID = AccommodationPackage.ID,
                AccommodationTypeID = AccommodationPackage.AccommodationTypeID,
                Name = AccommodationPackage.Name,
                NoOfRoom = AccommodationPackage.NoofRoom,
                FeePerNight = AccommodationPackage.FeePerNight

            };

            // Zwróć widok w formie częściowego widoku (partial view)
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public JsonResult Delete(AccommodationPackageActionModel model)
        {
            if (model == null || model.ID <= 0)
            {
                return Json(new { success = false, message = "Nieprawidłowe dane wejściowe." });
            }

            var AccommodationPackage = _AccommodationPackagesService.GetAccommodationPackageByID(model.ID);

            if (AccommodationPackage == null)
            {
                return Json(new { success = false, message = "Nie znaleziono typu zakwaterowania o podanym ID." });
            }

            var result = _AccommodationPackagesService.DeleteAccommodationPackage(AccommodationPackage.ID);

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
