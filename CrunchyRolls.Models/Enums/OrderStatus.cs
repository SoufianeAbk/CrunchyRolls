namespace CrunchyRolls.Models.Enums
{
    /// <summary>
    /// Definieert de mogelijke statussen van een bestelling in het CrunchyRolls systeem.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// De bestelling is ontvangen en wacht op verwerking.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// De bestelling wordt momenteel verwerkt in de keuken.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// De bestelling is onderweg naar de klant.
        /// </summary>
        Shipped = 2,

        /// <summary>
        /// De bestelling is succesvol afgeleverd.
        /// </summary>
        Delivered = 3,

        /// <summary>
        /// De bestelling is geannuleerd.
        /// </summary>
        Cancelled = 4
    }
}