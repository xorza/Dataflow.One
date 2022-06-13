using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace csso.NodeRunner.UI;

public enum NullVisibilityConverterOptions {
    NullToCollapsed,
    NullToHidden
}

public class NullVisibilityConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        var nullVisibility = Visibility.Collapsed;
        if (parameter is NullVisibilityConverterOptions option)
            if (NullVisibilityConverterOptions.NullToHidden == option)
                nullVisibility = Visibility.Hidden;

        return value == null ? nullVisibility : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}