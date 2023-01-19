using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace csso.Nodeshop.UI;

public class ClickControl : Control {
    private bool _leftBuffonDown;

    protected ClickControl() {
        MouseEnter += MouseEnterHandler;
        MouseLeave += MouseEnterHandler;
        MouseLeftButtonDown += MouseLeftButtonDownHandler;
        MouseLeftButtonUp += MouseLeftButtonUpHandler;
    }

    public event MouseButtonEventHandler? LeftButtonClick;

    private void MouseEnterHandler(object? sender, RoutedEventArgs ea) {
        _leftBuffonDown = false;

        InvalidateVisual();
    }

    private void MouseLeftButtonDownHandler(object? sender, MouseButtonEventArgs ea) {
        _leftBuffonDown = true;

        InvalidateVisual();
    }

    private void MouseLeftButtonUpHandler(object? sender, MouseButtonEventArgs ea) {
        if (_leftBuffonDown) {
            LeftButtonClick?.Invoke(sender, ea);
        }

        _leftBuffonDown = false;

        InvalidateVisual();
    }
}