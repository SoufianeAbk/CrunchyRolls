namespace CrunchyRolls.Models.Enums
{
    /// <summary>
    /// GDPR Consent types
    /// </summary>
    public enum ConsentType
    {
        /// <summary>
        /// Privacy Policy & Data Processing
        /// </summary>
        PrivacyPolicy = 0,

        /// <summary>
        /// Marketing & Newsletter
        /// </summary>
        Marketing = 1,

        /// <summary>
        /// Cookies & Analytics
        /// </summary>
        Cookies = 2,

        /// <summary>
        /// Terms & Conditions
        /// </summary>
        TermsConditions = 3,

        /// <summary>
        /// Data Processing Agreement
        /// </summary>
        DataProcessing = 4
    }
}