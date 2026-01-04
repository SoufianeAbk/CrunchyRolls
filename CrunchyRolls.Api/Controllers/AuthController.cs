using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using CrunchyRolls.Core.Authentication.Models;
using CrunchyRolls.Models.Entities;
using CrunchyRolls.Data.Repositories;

namespace CrunchyRolls.Api.Controllers
{
    /// <summary>
    /// Authentication controller voor JWT token generation
    /// Gebruikt database users met hashed passwords
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IUserRepository userRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Login endpoint
        /// POST: /api/auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"🔐 Login attempt for {request.Email}");
                Debug.WriteLine($"🔐 AuthController.Login called for {request.Email}");

                // Valideer input
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    Debug.WriteLine("❌ Email or password is empty");
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email en wachtwoord zijn verplicht"
                    });
                }

                // ===== Get user from database =====
                var user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null || !user.IsActive)
                {
                    Debug.WriteLine($"❌ User not found or inactive: {request.Email}");
                    _logger.LogWarning($"❌ Login attempt for non-existent or inactive user: {request.Email}");

                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Ongeldig email adres of wachtwoord"
                    });
                }

                // ===== Verify password =====
                if (!_userRepository.VerifyPassword(request.Password, user.PasswordHash))
                {
                    Debug.WriteLine($"❌ Invalid password for {request.Email}");
                    _logger.LogWarning($"❌ Invalid password attempt for {request.Email}");

                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Ongeldig email adres of wachtwoord"
                    });
                }

                // ===== Generate JWT Token =====
                var token = GenerateJwtToken(user);

                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.WriteLine("❌ Token generation failed");
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Token generatie mislukt"
                    });
                }

                // ===== Update last login =====
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // ===== Create response =====
                var authUser = new AuthUser
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CreatedDate = user.CreatedDate
                };

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Inloggen succesvol",
                    Token = token,
                    User = authUser
                };

                Debug.WriteLine($"✅ Login successful for {request.Email}");
                _logger.LogInformation($"✅ Login successful for {request.Email}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Exception in Login: {ex.Message}\n{ex.StackTrace}");
                _logger.LogError(ex, "Error in login endpoint");

                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Refresh expired token
        /// POST: /api/auth/refresh
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] dynamic request)
        {
            try
            {
                string? oldToken = request?.token;

                if (string.IsNullOrWhiteSpace(oldToken))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Token is verplicht"
                    });
                }

                var email = ExtractEmailFromToken(oldToken);

                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Ongeldig token"
                    });
                }

                // Get user from database to verify still exists
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Gebruiker niet gevonden of inactief"
                    });
                }

                var newToken = GenerateJwtToken(user);

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Token vernieuwd",
                    Token = newToken,
                    User = new AuthUser
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Token vernieuwing mislukt"
                });
            }
        }

        /// <summary>
        /// Generate JWT token for user
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            try
            {
                var jwtSecret = _configuration["Jwt:Secret"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];
                var jwtExpirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

                if (string.IsNullOrEmpty(jwtSecret))
                    jwtSecret = "VeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!@#";
                if (string.IsNullOrEmpty(jwtIssuer))
                    jwtIssuer = "CrunchyRolls";
                if (string.IsNullOrEmpty(jwtAudience))
                    jwtAudience = "CrunchyRollsApp";

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                Debug.WriteLine($"✅ JWT Token generated for {user.Email}");
                Debug.WriteLine($"   Token expires in {jwtExpirationMinutes} minutes");

                return tokenString;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error generating JWT: {ex.Message}");
                _logger.LogError(ex, "Error generating JWT token");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extract email from JWT token claims
        /// </summary>
        private string? ExtractEmailFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                    return null;

                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}