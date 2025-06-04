using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.Models;
using SkillBridge.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace SkillBridge.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static List<User> users = new List<User>();
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public IActionResult Register(UserDto request)
        {
            if (users.Any(u => u.Username == request.Username))
                return BadRequest("Bu kullanıcı adı zaten mevcut.");

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = request.Username,
                Role = "User", // veya "Admin" gibi ihtiyaca göre
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            users.Add(user);

            return Ok("Kayıt başarılı.");
        }

        [HttpPost("login")]
        public IActionResult Login(UserDto request)
        {
            var user = users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return BadRequest("Şifre hatalı.");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        // Şifre hash oluşturucu
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        // Şifre doğrulayıcı
        private bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }
    }
}