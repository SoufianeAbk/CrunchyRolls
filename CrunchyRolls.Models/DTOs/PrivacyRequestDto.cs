namespace CrunchyRolls.Models.DTOs
{
    /// <summary>
    /// User consent request DTO
    /// </summary>
    public class UserConsentRequestDto
    {
        public bool ConsentPrivacyPolicy { get; set; }
        public bool ConsentMarketing { get; set; }
        public bool ConsentCookies { get; set; }
        public bool ConsentTermsConditions { get; set; }
        public bool ConsentDataProcessing { get; set; }
    }

    /// <summary>
    /// User consent response DTO
    /// </summary>
    public class UserConsentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool ConsentPrivacyPolicy { get; set; }
        public bool ConsentMarketing { get; set; }
        public bool ConsentCookies { get; set; }
        public bool ConsentTermsConditions { get; set; }
        public bool ConsentDataProcessing { get; set; }
        public DateTime ConsentDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? PrivacyPolicyVersion { get; set; }
    }

    /// <summary>
    /// Account deletion request DTO
    /// </summary>
    public class AccountDeletionRequestDto
    {
        public string? Reason { get; set; }
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Data correction/rectification request DTO
    /// </summary>
    public class DataRectificationRequestDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Privacy policy response DTO
    /// </summary>
    public class PrivacyPolicyResponseDto
    {
        public string Policy { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Generic privacy response DTO
    /// </summary>
    public class PrivacyResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RequestId { get; set; }
        public List<string>? Notes { get; set; }
    }
}