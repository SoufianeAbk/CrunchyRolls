using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// ✅ Correct using statements for CrunchyRolls
using CrunchyRolls.Core.Authentication.Models;
using CrunchyRolls.Models.Entities;

namespace CrunchyRolls.Api.Controllers
{
    /// <summary>
    /// Authentication controller voor JWT token generation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        // Hardcoded test user (In production: query database)
        private static readonly Dictionary<string, string> TestUsers = new()
        {
            { "test@example.com", "Password123" }
        };

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

                // ===== DEVELOPMENT: Simple test user validation =====
                // In production: Query database user table
                if (!TestUsers.TryGetValue(request.Email, out var storedPassword) ||
                    storedPassword != request.Password)
                {
                    Debug.WriteLine($"❌ Invalid credentials for {request.Email}");
                    _logger.LogWarning($"❌ Invalid login attempt for {request.Email}");

                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Ongeldig email adres of wachtwoord"
                    });
                }

                // ===== Generate JWT Token =====
                var token = GenerateJwtToken(request.Email);

                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.WriteLine("❌ Token generation failed");
                    return StatusCode(500, new LoginResponse
                    {
                        Success = false,
                        Message = "Token generatie mislukt"
                    });
                }

                // ===== Create user object =====
                var user = new AuthUser
                {
                    Id = 1,
                    Email = request.Email,
                    FirstName = "Test",
                    LastName = "User",
                    Role = "Customer",
                    CreatedDate = DateTime.Now
                };

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Inloggen succesvol",
                    Token = token,
                    User = user
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

                
                var newToken = GenerateJwtToken(email);

                var response = new LoginResponse
                {
                    Success = true,
                    Message = "Token vernieuwd",
                    Token = newToken,
                    User = new AuthUser
                    {
                        Id = 1,
                        Email = email,
                        FirstName = "Test",
                        LastName = "User"
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

        
        private string GenerateJwtToken(string email)
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
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, email.Split('@')[0]),
                    new Claim("role", "Customer"),
                    new Claim(JwtRegisteredClaimNames.Sub, "1"),
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

                Debug.WriteLine($"✅ JWT Token generated for {email}");
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