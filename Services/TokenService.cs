using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureFileStorage.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly int _jwtExpirationInMinutes;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecret = _configuration["JwtSettings:Secret"];
            _jwtIssuer = _configuration["JwtSettings:Issuer"];
            _jwtExpirationInMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtIssuer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}