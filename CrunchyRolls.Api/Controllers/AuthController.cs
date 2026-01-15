using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using CrunchyRolls.Core.Authentication.Models;
using CrunchyRolls.Models.Entities;

namespace CrunchyRolls.Api.Controllers
{
    /// <summary>
    /// Authentication controller met ASP.NET Core Identity Framework
    /// Gebruikt UserManager en SignInManager voor user management
    /// Genereert JWT tokens voor API authenticatie
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        /// <summary>
        /// Register new user
        /// POST: /api/auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"📝 Registration attempt for {request.Email}");

                // Validatie
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Alle velden zijn verplicht"
                    });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email adres is al in gebruik"
                    });
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Email,  // Use email as username
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                // Create user with password (Identity handles hashing)
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning($"❌ Registration failed for {request.Email}: {errors}");

                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = $"Registratie mislukt: {errors}"
                    });
                }

                // Add to Customer role by default
                await _userManager.AddToRoleAsync(user, "Customer");

                // Generate JWT token
                var token = await GenerateJwtTokenAsync(user);

                var authUser = new AuthUser
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = "Customer",
                    CreatedDate = user.CreatedDate
                };

                _logger.LogInformation($"✅ User registered successfully: {request.Email}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Registratie succesvol",
                    Token = token,
                    User = authUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Login endpoint met Identity Framework
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

                // ===== Find user by email (Identity UserManager) =====
                var user = await _userManager.FindByEmailAsync(request.Email);

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

                // ===== Verify password (Identity SignInManager) =====
                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    lockoutOnFailure: true);  // Enable lockout after failed attempts

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        Debug.WriteLine($"🔒 Account locked out: {request.Email}");
                        _logger.LogWarning($"🔒 Account locked out: {request.Email}");

                        return Unauthorized(new LoginResponse
                        {
                            Success = false,
                            Message = "Account tijdelijk vergrendeld. Probeer later opnieuw."
                        });
                    }

                    Debug.WriteLine($"❌ Invalid password for {request.Email}");
                    _logger.LogWarning($"❌ Invalid password attempt for {request.Email}");

                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Ongeldig email adres of wachtwoord"
                    });
                }

                // ===== Generate JWT Token =====
                var token = await GenerateJwtTokenAsync(user);

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
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // ===== Get user roles =====
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Customer";

                // ===== Create response =====
                var authUser = new AuthUser
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = userRole,
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

                // Get user from Identity
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Gebruiker niet gevonden of inactief"
                    });
                }

                // Generate new token
                var newToken = await GenerateJwtTokenAsync(user);

                // Get user role
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Customer";

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
                        Role = userRole
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
        /// Generate JWT token for user met Identity claims
        /// </summary>
        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
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

                // Get user roles from Identity
                var roles = await _userManager.GetRolesAsync(user);

                // Create claims list
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
                };

                // Add role claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                Debug.WriteLine($"✅ JWT Token generated for {user.Email}");
                Debug.WriteLine($"   Roles: {string.Join(", ", roles)}");
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

    /// <summary>
    /// Registration request model
    /// </summary>
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}