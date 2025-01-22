using HMS.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



public class UserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<UserDTO>> GetUsersAsync(string searchTerm, string roleID, int page, int recordSize)
    {
        page = page < 1 ? 1 : page;

        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => u.Email.Contains(searchTerm) || u.UserName.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(roleID))
        {
            var role = await _roleManager.FindByIdAsync(roleID);
            if (role != null)
            {
                var userIdsInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                query = query.Where(u => userIdsInRole.Select(ur => ur.Id).Contains(u.Id));
            }
        }

        var skip = (page - 1) * recordSize;
        var pagedUsers = await query.Skip(skip).Take(recordSize).ToListAsync();

        return await MapUsersToDTO(pagedUsers);
    }

    public async Task<int> GetUsersCountAsync(string searchTerm, string roleID)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => u.Email.Contains(searchTerm) || u.UserName.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(roleID))
        {
            var role = await _roleManager.FindByIdAsync(roleID);
            if (role != null)
            {
                var userIdsInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                query = query.Where(u => userIdsInRole.Select(ur => ur.Id).Contains(u.Id));
            }
        }

        return await query.CountAsync();
    }

    public async Task<UserDTO> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDTO
        {
            ID = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Country = user.Country ?? string.Empty,
            City = user.City ?? string.Empty,
            Address = user.Address ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "No Role"
        };
    }


    public async Task<(bool success, List<string> errors)> SaveOrUpdateUserAsync(UserDTO userDto)
    {
        var errors = new List<string>();
        User user;

        if (string.IsNullOrEmpty(userDto.ID)) 
        {
            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userDto.Username,
                Email = userDto.Email,
                FullName = userDto.FullName,
                Address = userDto.Address,
                City = userDto.City,
                Country = userDto.Country
            };

            var createResult = await _userManager.CreateAsync(user, "DefaultPassword123!"); 
            if (!createResult.Succeeded)
            {
                errors.AddRange(createResult.Errors.Select(e => e.Description));
                return (false, errors);
            }
        }
        else 
        {
            user = await _userManager.FindByIdAsync(userDto.ID);
            if (user == null)
            {
                errors.Add("User not found.");
                return (false, errors);
            }

            user.UserName = userDto.Username;
            user.Email = userDto.Email;
            user.FullName = userDto.FullName;
            user.Address = userDto.Address;
            user.City = userDto.City;
            user.Country = userDto.Country;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                errors.AddRange(updateResult.Errors.Select(e => e.Description));
                return (false, errors);
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var roleResult = await _userManager.AddToRoleAsync(user, userDto.Role);
        if (!roleResult.Succeeded)
        {
            errors.AddRange(roleResult.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, errors);
    }



    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    private async Task<List<UserDTO>> MapUsersToDTO(List<User> users)
    {
        var userDTOs = new List<UserDTO>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDTOs.Add(new UserDTO
            {
                ID = user.Id,
                Email = user.Email ?? string.Empty, // Obsługa NULL
                Username = user.UserName ?? string.Empty, // Obsługa NULL
                FullName = user.FullName ?? string.Empty, // Obsługa NULL
                Country = user.Country ?? string.Empty,  // Obsługa NULL
                City = user.City ?? string.Empty,        // Obsługa NULL
                Address = user.Address ?? string.Empty,  // Obsługa NULL
                Role = roles.FirstOrDefault() ?? "No Role"
            });
        }

        return userDTOs;
    }


}


