using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Nutritionist.Api.Configuration;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;
using Nutritionist.Api.Models;

namespace Nutritionist.Api.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, string? Token)> RegisterAsync(
        RegisterRequest request);

    Task<(bool Success, string Message, string? Token)> LoginAsync(
        LoginRequest request);
}

public sealed class AuthService : IAuthService
{
    private readonly NutritionDbContext _db;
    private readonly JwtSettings _jwt;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(
        NutritionDbContext db,
        JwtSettings jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<(bool Success, string Message, string? Token)> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (_db.Users.Any(x => x.Email == email))
            return (false, "An account with this email already exists.", null);

        if (!Enum.TryParse<UserRole>(
                request.Role,
                true,
                out var role))
        {
            role = UserRole.Client;
        }

        // Public registration must not be able to create an Admin account.
        if (role == UserRole.Admin)
            role = UserRole.Client;

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = role
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);

        if (role == UserRole.Client)
        {
            _db.ClientProfiles.Add(new ClientProfile
            {
                UserId = user.Id
            });
        }

        await _db.SaveChangesAsync();

        return (true, "Registration successful.", CreateToken(user));
    }

    public async Task<(bool Success, string Message, string? Token)> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = _db.Users.FirstOrDefault(x =>
            x.Email == email &&
            x.IsActive);

        if (user is null)
            return (false, "Invalid email or password.", null);

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
            return (false, "Invalid email or password.", null);

        user.LastLoginAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, "Login successful.", CreateToken(user));
    }

    private string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            new Claim(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}