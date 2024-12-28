using HMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Areas.Dashboard.Components
{
    public class AccommodationTypesViewComponent : ViewComponent
    {
        private readonly AccommodationTypesService _service;

        public AccommodationTypesViewComponent(AccommodationTypesService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var AccommodationTypes = _service.GetAllAccommodationTypes();
            return View(AccommodationTypes);
        }
    }
}
