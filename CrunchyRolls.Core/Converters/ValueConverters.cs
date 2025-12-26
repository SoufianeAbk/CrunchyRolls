using CrunchyRolls.Models.Enums;
using Microsoft.Maui.Graphics;
using System.Globalization;

namespace CrunchyRolls.Core.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    public class IsNotZeroConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue > 0;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Pending => Color.FromArgb("#FFA500"),      // Orange
                    OrderStatus.Processing => Color.FromArgb("#0000FF"),   // Blue
                    OrderStatus.Shipped => Color.FromArgb("#800080"),      // Purple
                    OrderStatus.Delivered => Color.FromArgb("#008000"),    // Green
                    OrderStatus.Cancelled => Color.FromArgb("#FF0000"),    // Red
                    _ => Color.FromArgb("#808080")                         // Gray
                };
            }
            return Color.FromArgb("#808080");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Pending => "In behandeling",
                    OrderStatus.Processing => "Wordt verwerkt",
                    OrderStatus.Shipped => "Onderweg",
                    OrderStatus.Delivered => "Afgeleverd",
                    OrderStatus.Cancelled => "Geannuleerd",
                    _ => status.ToString()
                };
            }
            return value?.ToString() ?? "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToCancelVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                // Toon verwijder knop voor alle bestellingen behalve Delivered
                return status != OrderStatus.Delivered;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}