namespace CrunchyRolls.Core.Authentication.Interfaces
{
    /// <summary>
    /// Interface voor JWT token beheer
    /// Behandelt token validatie, vervaldatum en parsing
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Controleren of token geldig is en niet verlopen
        /// </summary>
        bool IsTokenValid(string token);

        /// <summary>
        /// Vervaldatum ophalen uit token
        /// </summary>
        DateTime? GetTokenExpiration(string token);

        /// <summary>
        /// Resterende tijd tot token verloopt ophalen
        /// </summary>
        TimeSpan? GetTimeUntilExpiration(string token);

        /// <summary>
        /// Controleren of token binnenkort verloopt (binnen x minuten)
        /// </summary>
        bool IsTokenExpiringSoon(string token, int minutesThreshold = 5);

        /// <summary>
        /// Claim waarde uit token extraheren
        /// </summary>
        string? GetClaimValue(string token, string claimType);

        /// <summary>
        /// Gebruiker ID uit token ophalen
        /// </summary>
        int? GetUserId(string token);

        /// <summary>
        /// Gebruiker email uit token ophalen
        /// </summary>
        string? GetUserEmail(string token);

        /// <summary>
        /// Token handtekening valideren
        /// </summary>
        bool ValidateTokenSignature(string token);
    }
}