using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using api.src.data;
using api.src.models.entities;
using api.src.models.requests;
using api.src.models.responses;

namespace api.src.services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Checks if the user already exists, hashes the password, creates a new user record, and generates a JWT token for authentication.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>An AuthResponse containing the user data and JWT token.</returns>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password with BCrypt (work factor 12)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        // Create new user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoggedIn = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = GenerateJwtToken(user);

        // Return response with user data (excluding password hash)
        return new AuthResponse
        {
            User = new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                LastLoggedIn = user.LastLoggedIn
            },
            Token = token
        };
    }

    /// <summary>
    /// Generates a JWT token containing the user's ID and email as claims, signed with a secret key from configuration. The token includes standard claims like issuer, audience, and expiration.
    /// </summary>
    /// <param name="user">The user for whom the token is being generated.</param>
    /// <returns>A JWT token as a string.</returns>
    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("Jwt__Secret");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("Jwt__Issuer");
        var jwtAudience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("Jwt__Audience");
        var jwtExpirationMinutes = int.Parse(
            _configuration["Jwt:ExpirationMinutes"] ?? 
            Environment.GetEnvironmentVariable("Jwt__ExpirationMinutes") ?? 
            "60"
        );

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}