// File: ViewModels/DeleteRoleModel.cs
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class DeleteRoleModel
    {
        [Required(ErrorMessage = "The ID field is required.")]
        public string ID { get; set; }
    }
}
