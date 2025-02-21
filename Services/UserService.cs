using SecureFileStorage.DTOs;
using SecureFileStorage.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SecureFileStorage.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, ITokenService tokenService, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<bool> RegisterUser(UserRegisterDto userRegisterDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userRegisterDto.Email))
            {
                return false; // Email zaten kullanılıyor
            }

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password, salt);

            var user = new User
            {
                Name = userRegisterDto.Name,
                Surname = userRegisterDto.Surname,
                Email = userRegisterDto.Email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TokenDto> LoginUser(UserLoginDto userLoginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (user == null)
            {
                return null; // Kullanıcı bulunamadı
            }

            if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                return null; // Şifre yanlış
            }

            // JWT claim'leri oluştur
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JTI ekle
                //Rolleride ekleyebilirsiniz örnek :new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"),
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var jwtToken = new JwtSecurityToken(accessToken);
            var jwtId = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;

            // Refresh token'ı veritabanına kaydet
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                JwtId = jwtId,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7) // Örneğin 7 gün geçerli
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<TokenDto> RefreshToken(string refreshToken)
        {
            // Refresh token'ı veritabanında ara
            var refreshTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (refreshTokenEntity == null || refreshTokenEntity.IsUsed || refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiryDate <= DateTime.UtcNow)
            {
                return null; // Geçersiz refresh token
            }

            // Access token'ı doğrula
            //var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenEntity.JwtId);
            //if (principal == null)
            //{
            //    return null; // Geçersiz Access Token
            //}

            //  var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await GetUserByEmail((await _context.Users.FirstOrDefaultAsync(x => x.Id == refreshTokenEntity.UserId)).Email);

            if (user == null)
            {
                return null; // Kullanıcı bulunamadı
            }

            // Yeni access token ve refresh token oluştur
            var claims = new[]
            {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
         new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JTI ekle
    };

            var newAccessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var jwtToken = new JwtSecurityToken(newAccessToken);
            var jti = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;


            // Eski refresh token'ı kullanıldı olarak işaretle
            refreshTokenEntity.IsUsed = true;
            //refreshTokenEntity.IsRevoked = true; // İsterseniz revoke edebilirsiniz

            //Yeni Refresh Token Oluştur
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                JwtId = jti,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7) // Örneğin 7 gün geçerli
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);

            await _context.SaveChangesAsync();

            return new TokenDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
        private bool IsRefreshTokenValid(string refreshToken, User user)
        {
            //Veritabanından kullanıcının refresh tokenini al ve karşılaştır
            return true;
        }
    }
}
