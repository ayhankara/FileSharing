using SecureFileStorage.DTOs;
using SecureFileStorage.Models;

public interface IUserService
{
    Task<bool> RegisterUser(UserRegisterDto userRegisterDto);
    Task<TokenDto> LoginUser(UserLoginDto userLoginDto);
    Task<User> GetUserByEmail(string email);
    Task<TokenDto> RefreshToken(string refreshToken);
}