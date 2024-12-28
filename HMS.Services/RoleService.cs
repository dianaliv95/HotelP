using HMS.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<RoleDTO>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var roleDTOs = new List<RoleDTO>();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            roleDTOs.Add(new RoleDTO
            {
                ID = role.Id,
                Name = role.Name,
                UsersCount = usersInRole.Count
            });
        }

        return roleDTOs;
    }

    public async Task<List<RoleDTO>> SearchRolesAsync(string searchTerm)
    {
        var roles = await _roleManager.Roles
            .Where(r => r.Name.Contains(searchTerm))
            .ToListAsync();

        var roleDTOs = new List<RoleDTO>();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            roleDTOs.Add(new RoleDTO
            {
                ID = role.Id,
                Name = role.Name,
                UsersCount = usersInRole.Count
            });
        }

        return roleDTOs;
    }

    public async Task<RoleDTO> GetRoleByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return null;

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

        return new RoleDTO
        {
            ID = role.Id,
            Name = role.Name,
            UsersCount = usersInRole.Count
        };
    }

    public async Task<bool> SaveOrUpdateRoleAsync(RoleDTO roleDto)
    {
        if (string.IsNullOrEmpty(roleDto.ID))
        {
            // Tworzenie nowej roli
            var existingRole = await _roleManager.FindByNameAsync(roleDto.Name);
            if (existingRole != null)
            {
                _logger.LogWarning("Próba utworzenia roli, która już istnieje: {RoleName}", roleDto.Name);
                return false;
            }

            var newRole = new IdentityRole(roleDto.Name);
            var result = await _roleManager.CreateAsync(newRole);
            if (!result.Succeeded)
            {
                _logger.LogError("Nie udało się utworzyć roli: {RoleName}. Błędy: {Errors}", roleDto.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return result.Succeeded;
        }
        else
        {
            // Aktualizacja istniejącej roli
            var existingRole = await _roleManager.FindByIdAsync(roleDto.ID);
            if (existingRole == null)
            {
                _logger.LogWarning("Próba aktualizacji nieistniejącej roli o ID: {RoleId}", roleDto.ID);
                return false;
            }

            existingRole.Name = roleDto.Name;
            var result = await _roleManager.UpdateAsync(existingRole);
            if (!result.Succeeded)
            {
                _logger.LogError("Nie udało się zaktualizować roli: {RoleName}. Błędy: {Errors}", roleDto.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return result.Succeeded;
        }
    }

    public async Task<bool> DeleteRoleAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return false;

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            _logger.LogError("Nie udało się usunąć roli: {RoleName}. Błędy: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        return result.Succeeded;
    }
}
