using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HMS.Entities;


namespace Hotel.Areas.Dashboard.ViewModels
{
    public class RolesListingModel
    {
        public List<RoleDTO> Roles { get; set; }
        public string SearchTerm { get; set; }
    }

    public class RoleActionModel
    {
        public string ID { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        public string Name { get; set; }
    }

}


