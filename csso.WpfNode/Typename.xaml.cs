using System;
using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode;

public partial class Typename : UserControl {
    public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register
    (
        nameof(ValueType), typeof(Type), typeof(Typename),
        new PropertyMetadata(default(Type), PropertyChangedCallback)
    );

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((Typename) d).Refresh();
    }

    public Type? ValueType {
        get => (Type) GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }

    public Typename() {
        InitializeComponent();
    }


    private void Refresh() {
        if(ValueType == null) {
            TypenameTextBlock.Text = "null";
            return;
        }
        TypenameTextBlock.Text = ValueType.Name;
    }
}