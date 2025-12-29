namespace CrunchyRolls.Core.Authentication.Models
{
    /// <summary>
    /// Login verzoek naar API
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// E-mailadres van gebruiker
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Wachtwoord van gebruiker
        /// </summary>
        public string Password { get; set; } = string.Empty;

        public override string ToString() => $"Inlog: {Email}";
    }

    /// <summary>
    /// Login antwoord van API
    /// Bevat JWT token en gebruikersinformatie
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Was inloggen succesvol?
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Bericht (error of succesmeldng)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// JWT token voor authenticatie
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Ingelogde gebruiker informatie
        /// </summary>
        public AuthUser? User { get; set; }

        public override string ToString() => $"Inlog: {(Success ? "Succesvol" : "Mislukt")}";
    }

    /// <summary>
    /// Huidge ingelogde gebruiker informatie
    /// </summary>
    public class AuthUser
    {
        /// <summary>
        /// Unieke gebruiker ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// E-mailadres
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Voornaam
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Achternaam
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gebruikerrol (Customer, Staff, Admin)
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Datum wanneer account aangemaakt is
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Volledige naam (voornaam + achternaam)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        public override string ToString() => $"{FullName} ({Email})";
    }
}