using HMS.Entities;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Dodaj ten using
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hotel.Areas.Dashboard.Controllers
{
	[Area("Dashboard")]
	
	public class DashboardController : Controller
	{
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly DashboardService _dashboardService;

		// Logger
		private readonly ILogger<DashboardController> _logger;

		public DashboardController(
			IWebHostEnvironment hostingEnvironment,
			DashboardService dashboardService,
			ILogger<DashboardController> logger // wstrzykujemy logger
		)
		{
			_hostingEnvironment = hostingEnvironment;
			_dashboardService = dashboardService;
			_logger = logger; // przypisanie do pola prywatnego
		}

		public IActionResult Index()
		{
			// Prosty log informacyjny
			_logger.LogInformation("DashboardController.Index() invoked.");

			return View();
		}

		[HttpPost]
		public async Task<JsonResult> UploadPictures([FromForm] List<IFormFile> Picture)
		{
			// "Picture" odpowiada nazwie <input type="file" name="Picture" multiple />
			_logger.LogInformation("== UploadPictures() invoked with {Count} file(s) ==", Picture?.Count ?? 0);

			var resultList = new List<Picture>();

			// Folder docelowy w /wwwroot/images/site
			var targetFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "site");

			// Logujemy ścieżkę docelową
			_logger.LogInformation("Target folder: {TargetFolder}", targetFolder);

			if (!Directory.Exists(targetFolder))
			{
				Directory.CreateDirectory(targetFolder);
				_logger.LogInformation("Folder nie istniał - utworzyłem: {TargetFolder}", targetFolder);
			}

			// Sprawdzamy listę plików
			if (Picture != null && Picture.Count > 0)
			{
				foreach (var file in Picture)
				{
					_logger.LogInformation(
						"Rozpoczynam przetwarzanie pliku: {FileName}, rozmiar: {Size} bajtów",
						file.FileName,
						file.Length
					);

					if (file != null && file.Length > 0)
					{
						// Generujemy unikatową nazwę
						var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
						var filePath = Path.Combine(targetFolder, fileName);

						try
						{
							// Zapis fizyczny pliku
							using (var stream = new FileStream(filePath, FileMode.Create))
							{
								await file.CopyToAsync(stream);
							}
							_logger.LogInformation(
								"Plik {FileName} zapisany na dysku pod nazwą {NewName}",
								file.FileName,
								fileName
							);

							// Tworzymy encję Picture
							var dbPicture = new Picture
							{
								URL = fileName
								// ewentualnie inne pola np. Title, DateAdded, itp.
							};

							// Zapis w bazie
							bool isSaved = _dashboardService.SavePicture(dbPicture);
							if (isSaved)
							{
								_logger.LogInformation("Zapis do bazy się powiódł, ID={Id}", dbPicture.ID);
								resultList.Add(dbPicture);
							}
							else
							{
								_logger.LogError("Błąd podczas zapisu do bazy pliku {FileName}", fileName);
							}
						}
						catch (Exception ex)
						{
							// Logujemy wyjątek w razie niepowodzenia
							_logger.LogError(ex, "Wyjątek przy zapisywaniu pliku {FileName}", file.FileName);
						}
					}
				}
			}
			else
			{
				_logger.LogWarning("Nie przesłano żadnych plików w polu 'Picture'.");
			}

			// Zwracamy JSON z listą wgranych obiektów:
			var response = resultList.ConvertAll(x => new
			{
				id = x.ID,
				url = x.URL
			});

			// Tutaj jeszcze log, co zwracamy
			_logger.LogInformation(
				"Zwracam {Count} obiektów do front-endu (np. AJAX).",
				response.Count
			);

			return Json(response);
		}
	}
}
