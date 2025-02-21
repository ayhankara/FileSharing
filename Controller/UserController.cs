using Microsoft.AspNetCore.Mvc;

namespace SecureFileStorage.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterUser(userRegisterDto);

            if (result)
            {
                return Ok("Kullanıcı başarıyla kaydedildi.");
            }
            else
            {
                return BadRequest("Kullanıcı kaydı başarısız oldu. Email zaten kullanılıyor olabilir.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _userService.LoginUser(userLoginDto);

            if (token == null)
            {
                return Unauthorized("Email veya şifre yanlış.");
            }

            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto tokenDto)
        {
            if (tokenDto is null)
            {
                return BadRequest("Invalid client request");
            }

            var token = await _userService.RefreshToken(tokenDto.RefreshToken);

            if (token is null)
            {
                return BadRequest("Invalid refresh token or access token");
            }

            return Ok(token);
        }
    }
}