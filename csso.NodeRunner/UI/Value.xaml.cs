using System.Windows;
using System.Windows.Controls;

namespace csso.NodeRunner.UI;

public partial class Value : UserControl {
    public static readonly DependencyProperty ValueViewProperty = DependencyProperty.Register
    (
        nameof(ValueView), typeof(ValueView), typeof(Value),
        new PropertyMetadata(default(ValueView), PropertyChangedCallback)
    );

    public static readonly DependencyProperty EditableProperty = DependencyProperty.Register
    (
        nameof(Editable), typeof(bool), typeof(Value),
        new PropertyMetadata(default(bool), PropertyChangedCallback)
    );

    public bool Editable {
        get => (bool) GetValue(EditableProperty);
        set => SetValue(EditableProperty, value);
    }

    public Value() {
        InitializeComponent();
    }

    public ValueView? ValueView {
        get => GetValue(ValueViewProperty) as ValueView;
        set => SetValue(ValueViewProperty, value);
    }

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (Value) d;
        control.Refresh();
    }

    public void Refresh() {
        if (Editable) {
            ValueViewContentPresenter.Visibility = Visibility.Collapsed;
            ValueEditGrid.Visibility = Visibility.Visible;
        } else {
            ValueViewContentPresenter.Visibility = Visibility.Visible;
            ValueEditGrid.Visibility = Visibility.Collapsed;
        }
    }
}