using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode;

public partial class Value : UserControl {
    public static readonly DependencyProperty ValueViewProperty = DependencyProperty.Register(
        nameof(ValueView), typeof(ValueView), typeof(Value),
        new PropertyMetadata(default(ValueView), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        Value value = (Value) d;
        ValueView? valueView = e.NewValue as ValueView;
        
        
    }

    public ValueView? ValueView {
        get { return GetValue(ValueViewProperty) as ValueView; }
        set { SetValue(ValueViewProperty, value); }
    }

    public Value() {
        InitializeComponent();
    }
}