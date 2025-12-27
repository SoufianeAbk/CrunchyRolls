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
                    // Voor .NET MAUI MauiImage: gebruik ALLEEN de bestandsnaam
                    // NIET Resources/Images/ - dat wordt automatisch door MAUI afgehandeld
                    return ImageSource.FromFile(imagePath);
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