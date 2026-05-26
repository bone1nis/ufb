using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HackBackend.Config;
using HackBackend.Models;
using Microsoft.IdentityModel.Tokens;

namespace HackBackend.Auth;

public static class JwtHelper
{
    public static string Generate(User user)
    {
        var secret = AppDefaults.JwtSecret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim("name", user.Name),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(AppDefaults.JwtExpiresHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static TokenValidationParameters ValidationParameters()
    {
        var secret = AppDefaults.JwtSecret;
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
        };
    }
}
