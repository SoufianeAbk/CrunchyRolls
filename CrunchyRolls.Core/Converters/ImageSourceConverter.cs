using System.Globalization;
using Microsoft.Maui.Controls;

namespace CrunchyRolls.Core.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    // Controleer of het pad al het volledige pad bevat
                    if (imagePath.StartsWith("Resources/"))
                    {
                        return ImageSource.FromFile(imagePath);
                    }

                    // Voeg het Resources/Images/ prefix toe
                    var fullPath = $"Resources/Images/{imagePath}";
                    return ImageSource.FromFile(fullPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image: {imagePath} - {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}