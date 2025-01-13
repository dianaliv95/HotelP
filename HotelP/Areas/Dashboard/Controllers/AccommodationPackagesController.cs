﻿using HMS.Entities;
using HMS.Services;
using HMS.ViewModels;
using Hotel.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hotel.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class AccommodationPackagesController : Controller
    {
        private readonly AccommodationPackagesService _AccommodationPackagesService;
        private readonly AccommodationTypesService _AccommodationTypesService;
        private readonly DashboardService _dashboardService; // obsługa zapisu i odczytu zdjęć

        public AccommodationPackagesController(
            AccommodationPackagesService accommodationPackagesService,
            AccommodationTypesService accommodationTypesService,
            DashboardService dashboardService)
        {
            _AccommodationPackagesService = accommodationPackagesService;
            _AccommodationTypesService = accommodationTypesService;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public IActionResult Index(string searchterm, int? AccommodationTypeID, int page = 1, int recordSize = 10)
        {
            // Pobierz listę typów zakwaterowania (do filtra lub selecta)
            var accommodationTypes = _AccommodationTypesService.GetAllAccommodationTypes()
                .Select(a => new AccommodationTypeViewModel
                {
                    ID = a.ID,
                    Name = a.Name
                }).ToList();

            // Pobierz i zmapuj pakiety
            var accommodationPackages = _AccommodationPackagesService
                .SearchAccommodationPackages(searchterm, AccommodationTypeID, page, recordSize)
                .Select(p => new AccommodationPackageViewModel
                {
                    ID = p.ID,
                    Name = p.Name,
                    FeePerNight = p.FeePerNight,
                    AccommodationTypeID = p.AccommodationTypeID,
                    AccommodationTypeName = p.AccommodationType != null
                                            ? p.AccommodationType.Name
                                            : "N/A"
                })
                .ToList();

            // Stronicowanie
            int totalItems = accommodationPackages.Count();
            var pager = new Pager(totalItems, page, recordSize);

            // Model do widoku
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

            // Pobieramy listę typów zakwaterowania
            var accommodationTypes = _AccommodationTypesService.GetAllAccommodationTypes();
            model.AccommodationTypes = accommodationTypes.Select(a => new AccommodationTypeViewModel
            {
                ID = a.ID,
                Name = a.Name
            }).ToList();

            // Jeśli mamy ID, to wczytujemy pakiet do edycji
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

                    // Wczytujemy istniejące powiązane zdjęcia, jeśli są
                    // (o ile w GetAccommodationPackageByID mamy Include(...))
                    // Możemy je przypisać do model.AccommodationPackagePictures
                    if (accommodationPackage.AccomodationPackagePictures != null)
                    {
                        model.AccommodationPackagePictures = accommodationPackage.AccomodationPackagePictures.ToList();
                    }
                }
            }

            // Zwracamy partial view z formularzem
            return PartialView("_Action", model);
        }

        [HttpPost]
        public JsonResult Action(AccommodationPackageActionModel model)
        {
            if (model == null)
                return Json(new { success = false, message = "Model jest nullem." });

            if (string.IsNullOrEmpty(model.Name))
                return Json(new { success = false, message = "Pole 'Name' jest wymagane." });

            bool result;
            try
            {
                // Czy tworzymy nowy, czy edytujemy
                if (model.ID > 0)
                {
                    // EDYCJA
                    var package = _AccommodationPackagesService.GetAccommodationPackageByID(model.ID);
                    if (package == null)
                        return Json(new { success = false, message = "Nie znaleziono rekordu do edycji." });

                    // Aktualizacja podstawowych pól
                    package.AccommodationTypeID = model.AccommodationTypeID;
                    package.Name = model.Name;
                    package.NoofRoom = model.NoOfRoom;
                    package.FeePerNight = model.FeePerNight;

                    // Jeśli EF wczytał .AccomodationPackagePictures, to czyścimy i odtwarzamy
                    if (package.AccomodationPackagePictures == null)
                    {
                        package.AccomodationPackagePictures = new List<AccommodationPackagePicture>();
                    }
                    else
                    {
                        package.AccomodationPackagePictures.Clear();
                    }

                    // Jeżeli przekazano pictureIDs, to przypisujemy
                    if (!string.IsNullOrEmpty(model.PictureIDs))
                    {
                        var splittedIds = model.PictureIDs
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList();

                        var pictures = _dashboardService.GetPicturesByIDs(splittedIds);
                        foreach (var pic in pictures)
                        {
                            package.AccomodationPackagePictures.Add(new AccommodationPackagePicture
                            {
                                PictureID = pic.ID
                            });
                        }
                    }

                    // Zapis w DB
                    result = _AccommodationPackagesService.UpdateAccommodationPackage(package);
                }
                else
                {
                    // NOWY
                    var package = new AccommodationPackage
                    {
                        AccommodationTypeID = model.AccommodationTypeID,
                        Name = model.Name,
                        NoofRoom = model.NoOfRoom,
                        FeePerNight = model.FeePerNight,
                        // Inicjalizujemy pustą listę
                        AccomodationPackagePictures = new List<AccommodationPackagePicture>()
                    };

                    if (!string.IsNullOrEmpty(model.PictureIDs))
                    {
                        var splittedIds = model.PictureIDs
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList();

                        var pictures = _dashboardService.GetPicturesByIDs(splittedIds);
                        foreach (var pic in pictures)
                        {
                            package.AccomodationPackagePictures.Add(new AccommodationPackagePicture
                            {
                                PictureID = pic.ID
                            });
                        }
                    }

                    // Zapis nowego w DB
                    result = _AccommodationPackagesService.SaveAccommodationPackage(package);
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Wystąpił błąd podczas zapisywania danych." });
            }

            if (result)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Błąd podczas zapisywania danych." });
        }

        [HttpGet]
        public IActionResult Delete(int ID)
        {
            var AccommodationPackage = _AccommodationPackagesService.GetAccommodationPackageByID(ID);
            if (AccommodationPackage == null)
                return NotFound("Nie znaleziono typu zakwaterowania o podanym ID.");

            var model = new AccommodationPackageActionModel
            {
                ID = AccommodationPackage.ID,
                AccommodationTypeID = AccommodationPackage.AccommodationTypeID,
                Name = AccommodationPackage.Name,
                NoOfRoom = AccommodationPackage.NoofRoom,
                FeePerNight = AccommodationPackage.FeePerNight
            };

            return PartialView("_Delete", model);
        }

        [HttpPost]
        public JsonResult Delete(AccommodationPackageActionModel model)
        {
            if (model == null || model.ID <= 0)
                return Json(new { success = false, message = "Nieprawidłowe dane wejściowe." });

            var AccommodationPackage = _AccommodationPackagesService.GetAccommodationPackageByID(model.ID);
            if (AccommodationPackage == null)
                return Json(new { success = false, message = "Nie znaleziono typu zakwaterowania o podanym ID." });

            var result = _AccommodationPackagesService.DeleteAccommodationPackage(AccommodationPackage.ID);
            if (result)
                return Json(new { success = true, message = "Rekord został usunięty." });
            else
                return Json(new { success = false, message = "Błąd podczas usuwania rekordu." });
        }
    }
}
