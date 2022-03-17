using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode;

public partial class Value : UserControl {
    public static readonly DependencyProperty ValueViewProperty = DependencyProperty.Register(
        nameof(ValueView), typeof(ValueView), typeof(Value),
        new PropertyMetadata(default(ValueView), PropertyChangedCallback));

    public Value() {
        InitializeComponent();
    }

    public ValueView? ValueView {
        get => GetValue(ValueViewProperty) as ValueView;
        set => SetValue(ValueViewProperty, value);
    }

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var value = (Value) d;
        var valueView = e.NewValue as ValueView;
    }
}