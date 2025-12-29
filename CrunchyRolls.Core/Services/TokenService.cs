using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CrunchyRolls.Core.Authentication.Services
{
    /// <summary>
    /// Service voor JWT token validatie en parsing
    /// </summary>
    public class TokenService
    {
        public TokenService()
        {
            Debug.WriteLine("🔑 Token service geïnitialiseerd");
        }

        /// <summary>
        /// Controleren of token geldig is en niet verlopen
        /// </summary>
        public bool IsTokenValid(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Debug.WriteLine("⚠️ Token is leeg");
                return false;
            }

            try
            {
                // Token moet 3 delen hebben gescheiden door punten
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    Debug.WriteLine("❌ Ongeldig token formaat");
                    return false;
                }

                // Vervaldatum controleren
                var expiration = GetTokenExpiration(token);
                if (expiration == null)
                {
                    Debug.WriteLine("❌ Kan vervaldatum niet uitlezen");
                    return false;
                }

                var isValid = DateTime.UtcNow < expiration;

                Debug.WriteLine(isValid
                    ? $"✅ Token is geldig (verloopt: {expiration:g})"
                    : $"❌ Token verlopen op {expiration:g}");

                return isValid;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij token validatie: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Vervaldatum uit token payload ophalen
        /// </summary>
        public DateTime? GetTokenExpiration(string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var parts = token.Split('.');
                if (parts.Length < 2)
                    return null;

                // Payload decoderen (tweede deel)
                var payload = DecodeBase64Url(parts[1]);
                if (string.IsNullOrWhiteSpace(payload))
                    return null;

                // JSON parsen
                using (var doc = JsonDocument.Parse(payload))
                {
                    var root = doc.RootElement;

                    // JWT gebruikt "exp" claim voor vervaldatum (seconden sinds epoch)
                    if (root.TryGetProperty("exp", out var expProperty) &&
                        expProperty.TryGetInt64(out var expSeconds))
                    {
                        var expiration = UnixTimeStampToDateTime(expSeconds);
                        Debug.WriteLine($"🕐 Token verloopt: {expiration:g}");
                        return expiration;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen vervaldatum: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resterende tijd tot token verloopt ophalen
        /// </summary>
        public TimeSpan? GetTimeUntilExpiration(string? token)
        {
            try
            {
                var expiration = GetTokenExpiration(token);
                if (expiration == null)
                    return null;

                var remaining = expiration.Value - DateTime.UtcNow;
                Debug.WriteLine($"⏱️ Token verloopt over: {remaining.TotalMinutes:F1} minuten");

                return remaining;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij berekenen resterende tijd: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Controleren of token binnenkort verloopt (binnen drempel)
        /// </summary>
        public bool IsTokenExpiringSoon(string? token, int minutesThreshold = 5)
        {
            try
            {
                var remaining = GetTimeUntilExpiration(token);
                if (remaining == null)
                    return true; // Behandel als verlopen als we het niet kunnen uitlezen

                var expiringSoon = remaining.Value.TotalMinutes <= minutesThreshold;

                if (expiringSoon)
                {
                    Debug.WriteLine($"⚠️ Token verloopt binnenkort ({remaining.Value.TotalMinutes:F1} minuten)");
                }

                return expiringSoon;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Claim waarde uit token payload extraheren
        /// </summary>
        public string? GetClaimValue(string? token, string claimType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(claimType))
                    return null;

                var parts = token.Split('.');
                if (parts.Length < 2)
                    return null;

                var payload = DecodeBase64Url(parts[1]);
                if (string.IsNullOrWhiteSpace(payload))
                    return null;

                using (var doc = JsonDocument.Parse(payload))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty(claimType, out var property))
                    {
                        return property.GetString() ?? property.ToString();
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen claim {claimType}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gebruiker ID uit token ophalen (meestal in "sub" of "nameid" claim)
        /// </summary>
        public int? GetUserId(string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                // Probeer veelgebruikte claim namen
                var subClaim = GetClaimValue(token, "sub");
                if (!string.IsNullOrWhiteSpace(subClaim) && int.TryParse(subClaim, out var id))
                    return id;

                var nameIdClaim = GetClaimValue(token, "nameid");
                if (!string.IsNullOrWhiteSpace(nameIdClaim) && int.TryParse(nameIdClaim, out var id2))
                    return id2;

                var userIdClaim = GetClaimValue(token, "userid");
                if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var id3))
                    return id3;

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij ophalen gebruiker ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gebruiker email uit token ophalen
        /// </summary>
        public string? GetUserEmail(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            // Probeer veelgebruikte claim namen
            var email = GetClaimValue(token, "email");
            if (!string.IsNullOrWhiteSpace(email))
                return email;

            var emailAddress = GetClaimValue(token, "emailaddress");
            if (!string.IsNullOrWhiteSpace(emailAddress))
                return emailAddress;

            return null;
        }

        /// <summary>
        /// Gebruiker naam uit token ophalen
        /// </summary>
        public string? GetUserName(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            // Probeer veelgebruikte claim namen
            var name = GetClaimValue(token, "name");
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            var fullName = GetClaimValue(token, "fullname");
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;

            return null;
        }

        /// <summary>
        /// Gebruiker rol uit token ophalen
        /// </summary>
        public string? GetUserRole(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return GetClaimValue(token, "role");
        }

        /// <summary>
        /// Token handtekening valideren (basis controle)
        /// Volledige validatie vereist server geheim sleutel
        /// </summary>
        public bool ValidateTokenSignature(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return false;

                // Controleer dat alle delen geldige base64url zijn
                try
                {
                    DecodeBase64Url(parts[0]); // Header
                    DecodeBase64Url(parts[1]); // Payload
                    // Handtekening is ook base64url gecodeerd
                    var sigBytes = Base64UrlDecode(parts[2]);
                    return sigBytes.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Fout bij validatie handtekening: {ex.Message}");
                return false;
            }
        }

        // ===== PRIVATE HULPMETHODES =====

        private string? DecodeBase64Url(string base64url)
        {
            try
            {
                var bytes = Base64UrlDecode(base64url);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }

        private byte[] Base64UrlDecode(string base64url)
        {
            // Voeg padding toe indien nodig
            var padded = base64url.Length % 4 == 0
                ? base64url
                : base64url + new string('=', 4 - base64url.Length % 4);

            // Vervang base64url karakters door standaard base64
            var base64 = padded.Replace('-', '+').Replace('_', '/');

            return Convert.FromBase64String(base64);
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }
    }
}