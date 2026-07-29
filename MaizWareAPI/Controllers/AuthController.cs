using System.Security.Cryptography;
using System.Text;
using MaizWareAPI.DTOs;
using MaizWareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaizWareAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly MaizWareContext _context;

    public AuthController(MaizWareContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Correo y contrasena son obligatorios." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var passwordHash = HashPassword(request.Password);
        var user = await UserQuery().FirstOrDefaultAsync(item => item.Email == normalizedEmail);

        if (user is null || !user.IsActive || !SlowEquals(user.PasswordHash, passwordHash))
        {
            return BadRequest(new { message = "Correo o contrasena incorrectos." });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var refreshedUser = await UserQuery().AsNoTracking().FirstAsync(item => item.UserId == user.UserId);
        return Ok(new AuthResponse(MapUser(refreshedUser)));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Correo y contrasena son obligatorios." });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "La contrasena debe tener al menos 6 caracteres." });
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "Nombre y apellido son obligatorios." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _context.Users.AnyAsync(item => item.Email == normalizedEmail);

        if (exists)
        {
            return BadRequest(new { message = "Ya existe una cuenta con ese correo." });
        }

        var role = await _context.Roles.FirstOrDefaultAsync(item => item.RoleName == "Usuario");
        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = HashPassword(request.Password),
            IsActive = true,
            UserProfile = new UserProfile
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Phone = request.Phone
            }
        };

        if (role is not null)
        {
            user.UserRoles.Add(new UserRole { RoleId = role.RoleId });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var createdUser = await UserQuery().AsNoTracking().FirstAsync(item => item.UserId == user.UserId);
        return CreatedAtAction(nameof(Register), new { id = user.UserId }, new AuthResponse(MapUser(createdUser)));
    }

    private IQueryable<User> UserQuery() =>
        _context.Users
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

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private static bool SlowEquals(string currentHash, string incomingHash) =>
        string.Equals(currentHash.Trim(), incomingHash.Trim(), StringComparison.OrdinalIgnoreCase);
}
