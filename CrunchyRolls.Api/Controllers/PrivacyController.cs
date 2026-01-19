using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Models.Entities;
using CrunchyRolls.Models.DTOs;
using System.Diagnostics;
using System.Security.Claims;

namespace CrunchyRolls.Api.Controllers
{
    /// <summary>
    /// GDPR & Privacy Compliance Controller
    /// Handles user consent, data export, deletion, and privacy policies
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PrivacyController : ControllerBase
    {
        private readonly IUserConsentRepository _consentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PrivacyController> _logger;

        public PrivacyController(
            IUserConsentRepository consentRepository,
            IUserRepository userRepository,
            ILogger<PrivacyController> logger)
        {
            _consentRepository = consentRepository ?? throw new ArgumentNullException(nameof(consentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger;
        }

        /// <summary>
        /// GET: api/privacy/policy
        /// Get Privacy Policy (Public - No Auth Required)
        /// </summary>
        [HttpGet("policy")]
        [AllowAnonymous]
        public IActionResult GetPrivacyPolicy()
        {
            try
            {
                var privacyPolicy = @"# CrunchyRolls Privacy Policy

**Version:** 1.0  
**Last Updated:** January 19, 2026

## GDPR Compliance

This application is fully compliant with the General Data Protection Regulation (GDPR).

### Your Rights

1. **Right to Access** - Request all data we hold about you
2. **Right to Rectification** - Correct inaccurate data
3. **Right to Erasure** - Request deletion of your data (Right to be Forgotten)
4. **Right to Data Portability** - Receive your data in structured format (JSON/CSV)
5. **Right to Object** - Object to certain processing activities

### Data We Collect

- **Account Data**: Email, name, password (hashed)
- **Order Data**: Orders, delivery address, payment info
- **Usage Data**: Login times, IP address, device info
- **Consent Data**: Which consents you've accepted

### How We Use Your Data

- Processing orders and deliveries
- User authentication and security
- Compliance with legal obligations
- Improving our service (with consent)
- Marketing communications (with consent)

### Data Retention

- Account data: Until account deletion
- Orders: 7 years (legal requirement)
- Login logs: 90 days
- Marketing preferences: Until revoked

### Security

- Passwords hashed with BCrypt
- HTTPS encryption for all communication
- Secure token storage (iOS Keychain, Android Keystore)
- Regular security audits

### Contact

For privacy questions: privacy@crunchyrolls.app  
Data Protection Officer: dpo@crunchyrolls.app";

                return Ok(new PrivacyPolicyResponseDto
                {
                    Policy = privacyPolicy,
                    Version = "1.0",
                    LastUpdated = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving privacy policy");
                return StatusCode(500, new { message = "Error retrieving privacy policy" });
            }
        }

        /// <summary>
        /// GET: api/privacy/terms
        /// Get Terms & Conditions (Public - No Auth Required)
        /// </summary>
        [HttpGet("terms")]
        [AllowAnonymous]
        public IActionResult GetTermsConditions()
        {
            try
            {
                var terms = @"# CrunchyRolls Terms & Conditions

**Version:** 1.0  
**Effective Date:** January 19, 2026

## 1. Acceptance of Terms

By using CrunchyRolls, you accept these terms.

## 2. Use License

You may use CrunchyRolls for personal, non-commercial purposes only.

## 3. User Account

- You're responsible for your password
- You're liable for account activities
- Don't share your account with others

## 4. Orders

- Orders subject to stock availability
- Prices may change without notice
- We reserve right to refuse orders

## 5. Payment

- Payments processed through secure gateways
- Refunds processed within 10 business days

## 6. Delivery

- Delivery times are estimates, not guarantees
- We're not liable for delivery delays

## 7. Liability

- We're not liable for indirect damages
- Maximum liability = order amount

## 8. Changes

- We may modify these terms anytime
- Continued use = acceptance

## 9. Termination

- We may terminate accounts for violations

## 10. Contact

- support@crunchyrolls.app
- privacy@crunchyrolls.app";

                return Ok(new { terms = terms, version = "1.0" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving terms");
                return StatusCode(500, new { message = "Error retrieving terms" });
            }
        }

        /// <summary>
        /// POST: api/privacy/consent
        /// Save user consent preferences (Protected)
        /// </summary>
        [HttpPost("consent")]
        [Authorize]
        public async Task<IActionResult> SaveConsent([FromBody] UserConsentRequestDto request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return Unauthorized();

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                Debug.WriteLine($"💾 Saving consent for user {userId}");

                var userConsent = new UserConsent
                {
                    UserId = userId.Value,
                    ConsentPrivacyPolicy = request.ConsentPrivacyPolicy,
                    ConsentMarketing = request.ConsentMarketing,
                    ConsentCookies = request.ConsentCookies,
                    ConsentTermsConditions = request.ConsentTermsConditions,
                    ConsentDataProcessing = request.ConsentDataProcessing,
                    ConsentDate = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    PrivacyPolicyVersion = "1.0"
                };

                await _consentRepository.CreateOrUpdateAsync(userConsent);

                Debug.WriteLine($"✅ Consent saved for user {userId}");
                _logger.LogInformation($"User {userId} provided GDPR consent");

                return Ok(new PrivacyResponseDto
                {
                    Success = true,
                    Message = "Consent saved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving consent");
                return StatusCode(500, new PrivacyResponseDto
                {
                    Success = false,
                    Message = "Error saving consent"
                });
            }
        }

        /// <summary>
        /// GET: api/privacy/consent
        /// Get user's consent preferences (Protected)
        /// </summary>
        [HttpGet("consent")]
        [Authorize]
        public async Task<IActionResult> GetConsent()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return Unauthorized();

                var consent = await _consentRepository.GetByUserIdAsync(userId.Value);

                if (consent == null)
                    return NotFound(new { message = "No consent record found" });

                var response = new UserConsentResponseDto
                {
                    Id = consent.Id,
                    UserId = consent.UserId,
                    ConsentPrivacyPolicy = consent.ConsentPrivacyPolicy,
                    ConsentMarketing = consent.ConsentMarketing,
                    ConsentCookies = consent.ConsentCookies,
                    ConsentTermsConditions = consent.ConsentTermsConditions,
                    ConsentDataProcessing = consent.ConsentDataProcessing,
                    ConsentDate = consent.ConsentDate,
                    LastUpdated = consent.LastUpdated,
                    PrivacyPolicyVersion = consent.PrivacyPolicyVersion
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consent");
                return StatusCode(500, new { message = "Error retrieving consent" });
            }
        }

        /// <summary>
        /// POST: api/privacy/export-data
        /// Request data export (GDPR Art. 20 - Right to Portability)
        /// </summary>
        [HttpPost("export-data")]
        [Authorize]
        public async Task<IActionResult> RequestDataExport()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return Unauthorized();

                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                    return NotFound();

                Debug.WriteLine($"📤 Data export requested by user {userId}");

                await _consentRepository.RequestDataExportAsync(userId.Value);

                _logger.LogInformation($"User {userId} requested data export");

                return Ok(new PrivacyResponseDto
                {
                    Success = true,
                    Message = "Data export request received. Check your email within 24-48 hours for download link.",
                    RequestId = Guid.NewGuid().ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data export");
                return StatusCode(500, new PrivacyResponseDto
                {
                    Success = false,
                    Message = "Error processing data export"
                });
            }
        }

        /// <summary>
        /// POST: api/privacy/delete-account
        /// Request account deletion (GDPR Art. 17 - Right to be Forgotten)
        /// </summary>
        [HttpPost("delete-account")]
        [Authorize]
        public async Task<IActionResult> RequestAccountDeletion([FromBody] AccountDeletionRequestDto request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return Unauthorized();

                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                    return NotFound();

                Debug.WriteLine($"🗑️ Account deletion requested by user {userId}");
                Debug.WriteLine($"   Reason: {request.Reason}");

                // Mark account for deletion
                user.IsActive = false;
                await _userRepository.UpdateAsync(user);

                // Mark consent record
                await _consentRepository.RequestDataDeletionAsync(userId.Value);

                _logger.LogInformation($"User {userId} requested account deletion. Reason: {request.Reason}");

                return Ok(new PrivacyResponseDto
                {
                    Success = true,
                    Message = "Account deletion request received. Your account will be permanently deleted within 30 days.",
                    RequestId = Guid.NewGuid().ToString(),
                    Notes = new List<string>
                    {
                        "✅ Account data will be anonymized",
                        "❌ Order history will be kept for 7 years (tax/legal requirement)",
                        "✅ Personal profile data will be deleted",
                        "✅ You will receive a confirmation email"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing account deletion");
                return StatusCode(500, new PrivacyResponseDto
                {
                    Success = false,
                    Message = "Error processing account deletion"
                });
            }
        }

        /// <summary>
        /// POST: api/privacy/rectify-data
        /// Request data correction (GDPR Art. 16 - Right to Rectification)
        /// </summary>
        [HttpPost("rectify-data")]
        [Authorize]
        public async Task<IActionResult> RectifyData([FromBody] DataRectificationRequestDto request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                    return Unauthorized();

                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                    return NotFound();

                Debug.WriteLine($"✏️ Data correction requested by user {userId}");
                Debug.WriteLine($"   Field: {request.FieldName}");

                // Update the specified field
                switch (request.FieldName.ToLower())
                {
                    case "firstname":
                        user.FirstName = request.NewValue;
                        break;
                    case "lastname":
                        user.LastName = request.NewValue;
                        break;
                    case "email":
                        return BadRequest(new { message = "Email updates require verification. Contact support." });
                    default:
                        return BadRequest(new { message = $"Cannot update field: {request.FieldName}" });
                }

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation($"User {userId} corrected {request.FieldName}");

                return Ok(new PrivacyResponseDto
                {
                    Success = true,
                    Message = "Data correction applied",
                    RequestId = request.FieldName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data rectification");
                return StatusCode(500, new PrivacyResponseDto
                {
                    Success = false,
                    Message = "Error processing data rectification"
                });
            }
        }

        // ===== HELPER METHODS =====

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
    }
}