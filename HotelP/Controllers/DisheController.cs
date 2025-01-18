using HMS.Services;
using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelP.Controllers
{
	public class DisheController : Controller
	{
		private readonly CategoryService _categoryService;
		private readonly HMSContext _context;

		public DisheController(CategoryService categoryService,HMSContext context)
		{
			_context = context;
			_categoryService = categoryService;
		}

		// GET: /Dishes/AllCategoriesDishes
		[HttpGet]
		public IActionResult AllCategoriesDishes()
		{
			var categories = _context.Categories
				.Include(c => c.Dishes)
					.ThenInclude(d => d.DishPictures)
					.ThenInclude(dp => dp.Picture)
				.ToList();

			return View(categories);
		}
	}
}
