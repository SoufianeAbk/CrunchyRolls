namespace CrunchyRolls.Models.Enums
{
    /// <summary>
    /// Gebruikerrollen voor autorisatie
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Normale klant - kan eigen bestellingen zien
        /// </summary>
        Customer = 0,

        /// <summary>
        /// Medewerker - kan bestellingen verwerken
        /// </summary>
        Staff = 1,

        /// <summary>
        /// Beheerder - volledige toegang
        /// </summary>
        Admin = 2
    }

    /// <summary>
    /// Hulp extensie methodes voor UserRole
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Rol weergavenaam ophalen (Nederlands)
        /// </summary>
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.Customer => "Klant",
                UserRole.Staff => "Medewerker",
                UserRole.Admin => "Beheerder",
                _ => "Onbekend"
            };
        }

        /// <summary>
        /// Rol string omzetten naar enum
        /// </summary>
        public static UserRole ParseRole(string? roleString)
        {
            if (string.IsNullOrWhiteSpace(roleString))
                return UserRole.Customer;

            return roleString.ToLower() switch
            {
                "admin" => UserRole.Admin,
                "beheerder" => UserRole.Admin,
                "staff" => UserRole.Staff,
                "medewerker" => UserRole.Staff,
                "customer" or "klant" or _ => UserRole.Customer
            };
        }
    }
}