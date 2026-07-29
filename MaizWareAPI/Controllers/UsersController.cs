using System.Security.Cryptography;
using System.Text;
using MaizWareAPI.DTOs;
using MaizWareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaizWareAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly MaizWareContext _context;

    public UsersController(MaizWareContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await UserQuery().OrderBy(user => user.UserId).ToListAsync();
        return Ok(users.Select(MapUser));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await UserQuery().FirstOrDefaultAsync(item => item.UserId == id);
        return user is null ? NotFound(new { message = "Usuario no encontrado." }) : Ok(MapUser(user));
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(role => role.RoleId)
            .Select(role => new RoleDto(role.RoleId, role.RoleName, role.Description))
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> RegisterUser(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "El correo es obligatorio." });
        }

        if (request.Profile is null || string.IsNullOrWhiteSpace(request.Profile.FirstName) || string.IsNullOrWhiteSpace(request.Profile.LastName))
        {
            return BadRequest(new { message = "El perfil debe incluir nombre y apellido." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(user => user.Email == normalizedEmail);

        if (emailExists)
        {
            return BadRequest(new { message = "Ya existe un usuario con ese correo." });
        }

        var roleResolution = await ResolveRoleIds(request.RoleIds);
        if (roleResolution.Error is not null)
        {
            return BadRequest(new { message = roleResolution.Error });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "La contrasena es obligatoria." });
        }

        var passwordHash = HashPassword(request.Password);

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            IsActive = true,
            UserProfile = new UserProfile
            {
                FirstName = request.Profile.FirstName.Trim(),
                LastName = request.Profile.LastName.Trim(),
                Phone = request.Profile.Phone,
                BirthDate = request.Profile.BirthDate,
                Gender = request.Profile.Gender,
                EmergencyContactName = request.Profile.EmergencyContactName,
                EmergencyContactPhone = request.Profile.EmergencyContactPhone
            },
            UserRoles = roleResolution.RoleIds.Select(roleId => new UserRole { RoleId = roleId }).ToList()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createdUser = await UserQuery().FirstAsync(item => item.UserId == user.UserId);
        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, MapUser(createdUser));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(item => item.UserProfile)
            .Include(item => item.UserRoles)
            .FirstOrDefaultAsync(item => item.UserId == id);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "El correo es obligatorio." });
        }

        if (request.Profile is null || string.IsNullOrWhiteSpace(request.Profile.FirstName) || string.IsNullOrWhiteSpace(request.Profile.LastName))
        {
            return BadRequest(new { message = "El perfil debe incluir nombre y apellido." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(item => item.Email == normalizedEmail && item.UserId != id);

        if (emailExists)
        {
            return BadRequest(new { message = "Ya existe otro usuario con ese correo." });
        }

        user.Email = normalizedEmail;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.UserProfile is null)
        {
            user.UserProfile = new UserProfile { UserId = user.UserId, FirstName = request.Profile.FirstName, LastName = request.Profile.LastName };
        }

        user.UserProfile.FirstName = request.Profile.FirstName.Trim();
        user.UserProfile.LastName = request.Profile.LastName.Trim();
        user.UserProfile.Phone = request.Profile.Phone;
        user.UserProfile.BirthDate = request.Profile.BirthDate;
        user.UserProfile.Gender = request.Profile.Gender;
        user.UserProfile.EmergencyContactName = request.Profile.EmergencyContactName;
        user.UserProfile.EmergencyContactPhone = request.Profile.EmergencyContactPhone;

        if (request.RoleIds is not null)
        {
            var roleResolution = await ResolveRoleIds(request.RoleIds);
            if (roleResolution.Error is not null)
            {
                return BadRequest(new { message = roleResolution.Error });
            }

            _context.UserRoles.RemoveRange(user.UserRoles);
            user.UserRoles = roleResolution.RoleIds.Select(roleId => new UserRole { UserId = id, RoleId = roleId }).ToList();
        }

        await _context.SaveChangesAsync();

        var updatedUser = await UserQuery().FirstAsync(item => item.UserId == id);
        return Ok(MapUser(updatedUser));
    }

    [HttpPost("{id:int}/roles")]
    public async Task<ActionResult<UserDto>> AssignRoles(int id, AssignRolesRequest request)
    {
        var user = await _context.Users.Include(item => item.UserRoles).FirstOrDefaultAsync(item => item.UserId == id);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        var roleResolution = await ResolveRoleIds(request.RoleIds);
        if (roleResolution.Error is not null)
        {
            return BadRequest(new { message = roleResolution.Error });
        }

        _context.UserRoles.RemoveRange(user.UserRoles);
        user.UserRoles = roleResolution.RoleIds.Select(roleId => new UserRole { UserId = id, RoleId = roleId }).ToList();
        await _context.SaveChangesAsync();

        var updatedUser = await UserQuery().FirstAsync(item => item.UserId == id);
        return Ok(MapUser(updatedUser));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<User> UserQuery() =>
        _context.Users
            .AsNoTracking()
            .Include(user => user.UserProfile)
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role);

    private static UserDto MapUser(User user) =>
        new(
            user.UserId,
            user.Email,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            user.UserProfile is null
                ? null
                : new UserProfileDto(
                    user.UserProfile.UserProfileId,
                    user.UserProfile.FirstName,
                    user.UserProfile.LastName,
                    user.UserProfile.Phone,
                    user.UserProfile.BirthDate,
                    user.UserProfile.Gender,
                    user.UserProfile.EmergencyContactName,
                    user.UserProfile.EmergencyContactPhone),
            user.UserRoles
                .Select(userRole => new RoleDto(userRole.Role.RoleId, userRole.Role.RoleName, userRole.Role.Description))
                .OrderBy(role => role.RoleId)
                .ToList());

    private async Task<(IReadOnlyCollection<int> RoleIds, string? Error)> ResolveRoleIds(IReadOnlyCollection<int>? requestedRoleIds)
    {
        if (requestedRoleIds is { Count: > 0 })
        {
            var distinctRoleIds = requestedRoleIds.Distinct().ToList();
            var existingRoleIds = await _context.Roles
                .Where(role => distinctRoleIds.Contains(role.RoleId))
                .Select(role => role.RoleId)
                .ToListAsync();

            return existingRoleIds.Count == distinctRoleIds.Count
                ? (existingRoleIds, null)
                : (Array.Empty<int>(), "Uno o mas roles no existen.");
        }

        var defaultRole = await _context.Roles.FirstOrDefaultAsync(role => role.RoleName == "Usuario");
        return defaultRole is null
            ? (Array.Empty<int>(), null)
            : (new[] { defaultRole.RoleId }, null);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}

