using Microsoft.AspNetCore.Identity;

namespace HMS.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; } // Nullable
        public string? Address { get; set; }  // Nullable
        public string? City { get; set; }     // Nullable
        public string? Country { get; set; }  // Nullable

    }

}
