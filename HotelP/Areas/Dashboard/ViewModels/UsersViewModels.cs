using HMS.Entities;
using HMS.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Areas.Dashboard.ViewModels
{
    public class UsersListingModel
    {
        public List<UserDTO> Users { get; set; } = new List<UserDTO>();
        public string SearchTerm { get; set; }
        public string RoleID { get; set; }
        public List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
        public Pager Pager { get; set; } = new Pager(0, 1, 10);
    }

    public class UserActionModel
    {
        public string ID { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } 

        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters.")]
        public string Country { get; set; } 

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } 

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; }

        public List<RoleDTO> AvailableRoles { get; set; } 
    }

}
