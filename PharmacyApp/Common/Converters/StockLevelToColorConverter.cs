using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PharmacyApp.Features.Products_Catalogue.ViewModels;

namespace PharmacyApp.Common.Converters
{
    public class StockLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is StockLevel level)
            {
                return level switch
                {
                    StockLevel.OutOfStock => new SolidColorBrush(Colors.Red),
                    StockLevel.LowStock => new SolidColorBrush(Colors.Orange),
                    StockLevel.InStock => new SolidColorBrush(Colors.Green),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        // We don't need ConvertBack for one-way bindings
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}