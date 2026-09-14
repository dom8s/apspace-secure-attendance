using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using FYP_AttendanceAPI.Data;
using FYP_AttendanceAPI.DTOs;

namespace FYP_AttendanceAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext    _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db     = db;
            _config = config;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Find user by TP number
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.TPNumber == request.TPNumber && u.IsActive);

            if (user is null) return null;

            // Verify password using BCrypt
            bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!passwordValid) return null;

            // Build JWT token
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("FYP_SecureAttendance_SuperSecretKey_2024!@#"));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry  = DateTime.UtcNow.AddMinutes(
                              double.Parse(_config["Jwt:ExpiryMinutes"] ?? "120"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.TPNumber),
                new Claim(ClaimTypes.Role,           user.Role),
                new Claim("FullName",                user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            expiry,
                signingCredentials: creds
            );

            return new LoginResponse
            {
                Token    = new JwtSecurityTokenHandler().WriteToken(token),
                TPNumber = user.TPNumber,
                FullName = user.FullName,
                Role     = user.Role,
                Expiry   = expiry
            };
        }
    }
}
