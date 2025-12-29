using CrunchyRolls.Core.Authentication.Models;

namespace CrunchyRolls.Core.Authentication.Interfaces
{
    /// <summary>
    /// Interface voor authenticatie operaties
    /// Behandelt inloggen, uitloggen en sessie beheer
    /// </summary>
    public interface IAuthService
    {
        // ===== EIGENSCHAPPEN =====

        /// <summary>
        /// Is de gebruiker momenteel ingelogd?
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Huidige ingelogde gebruiker (null als niet ingelogd)
        /// </summary>
        AuthUser? CurrentUser { get; }

        /// <summary>
        /// Huidge JWT token (null als niet ingelogd)
        /// </summary>
        string? CurrentToken { get; }

        // ===== EVENTS =====

        /// <summary>
        /// Event wanneer gebruiker inlogt
        /// </summary>
        event EventHandler<AuthUser>? LoginSucceeded;

        /// <summary>
        /// Event wanneer gebruiker uitlogt
        /// </summary>
        event EventHandler? LogoutCompleted;

        /// <summary>
        /// Event wanneer authenticatie faalt
        /// </summary>
        event EventHandler<string>? AuthenticationFailed;

        // ===== METHODES =====

        /// <summary>
        /// Inloggen met email en wachtwoord
        /// </summary>
        Task<LoginResponse> LoginAsync(string email, string password);

        /// <summary>
        /// Hurige gebruiker uitloggen
        /// </summary>
        Task<bool> LogoutAsync();

        /// <summary>
        /// Controleren of gebruiker nog ingelogd is
        /// Laad uit opslag indien nodig
        /// </summary>
        Task<bool> CheckAuthenticationStatusAsync();

        /// <summary>
        /// Token vernieuwen (als verlopen)
        /// </summary>
        Task<bool> RefreshTokenAsync();

        /// <summary>
        /// Authenticatie initialiseren (bij app start)
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Controleren of huige token geldig is
        /// </summary>
        bool IsTokenValid();
    }
}