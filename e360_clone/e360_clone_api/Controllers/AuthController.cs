using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace e360_clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IConfiguration _config;

        public AuthController(IAccountRepository accountRepository, IConfiguration config)
        {
            _accountRepository = accountRepository;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            // Find account by email or username
            var account = await _accountRepository.FindByEmailOrUsernameAsync(model.Email);

            if (account == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Check status
            if (account.Status != "Active")
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Your account has been locked or deactivated"
                });
            }

            // Verify password
            if (!PasswordHelper.VerifyPassword(model.Password, account.PasswordHash))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate JWT token
            var token = GenerateJwtToken(account);

            // Update last login
            account.LastLoginAt = DateTime.UtcNow;
            await _accountRepository.UpdateLastLoginAsync(account.Id);

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = new LoginResponse
                {
                    Token = token,
                    Username = account.Username,
                    Email = account.Email,
                    FullName = account.FullName ?? account.Username,
                    Role = account.Role,
                    AvatarUrl = account.AvatarUrl
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (await _accountRepository.EmailExistsAsync(model.Email))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Email đã tồn tại" });

            var username = model.Email.Split('@')[0];
            if (await _accountRepository.UsernameExistsAsync(username))
                username = username + "_" + DateTime.UtcNow.Ticks.ToString()[^4..];

            var account = new Account
            {
                Username = username,
                Email = model.Email,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = "Student",
                FullName = model.FullName,
                StudentId = model.StudentId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.AddAsync(account);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Tạo tài khoản thành công",
                Data = new { account.Id, account.Username, account.Email, account.FullName, account.Role }
            });
        }

        [HttpPost("quick-login")]
        public async Task<IActionResult> QuickLogin([FromBody] QuickLoginRequest model)
        {
            var username = $"{model.Role}@demo.com";
            
            // For demo: create a token without checking DB
            var demoAccount = new Account
            {
                Username = username,
                Email = username,
                Role = model.Role,
                FullName = $"Demo {model.Role}"
            };

            var token = GenerateJwtToken(demoAccount);

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Quick login successful",
                Data = new LoginResponse
                {
                    Token = token,
                    Username = username,
                    Email = username,
                    FullName = demoAccount.FullName,
                    Role = model.Role,
                    AvatarUrl = null
                }
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unauthorized"
                });
            }

            var account = await _accountRepository.GetByIdAsync(userId);
            if (account == null)
            {
                return HandleNotFound("Không tìm thấy tài khoản");
            }

            return HandleResult(new UserProfileResponse
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                FullName = account.FullName ?? account.Username,
                Role = account.Role,
                AvatarUrl = account.AvatarUrl,
                Status = account.Status
            }, "Lấy thông tin người dùng thành công");
        }

        private string GenerateJwtToken(Account account)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var issuer = jwtSettings["Issuer"] ?? "e360-clone";
            var audience = jwtSettings["Audience"] ?? "e360-clone-users";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role),
                new Claim("FullName", account.FullName ?? account.Username),
                new Claim("UserId", account.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public int? StudentId { get; set; }
    }

    public class QuickLoginRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
    }
}
