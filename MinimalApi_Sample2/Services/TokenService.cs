using Microsoft.IdentityModel.Tokens;
using MinimalApi_Sample2.Models;
using MinimalApi_Sample2.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimalApi_Sample2.Services
{
    public class TokenService : ITokenService 
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _securityKey;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _securityKey = GetSecurityKey(_configuration["JWT:SigningKey"]);
        }
        public string GenerateToken(User user)
        {
            var signingCredential = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature);

            Claim[] claims = [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserName),
            ];

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(24),
                SigningCredentials = signingCredential,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static SymmetricSecurityKey GetSecurityKey(string secretKey)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }
    }
}
