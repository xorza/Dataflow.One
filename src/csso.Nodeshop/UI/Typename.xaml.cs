using System;
using System.Windows;
using System.Windows.Controls;

namespace csso.Nodeshop.UI;

public partial class Typename : UserControl {
    public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register
    (
        nameof(ValueType), typeof(Type), typeof(Typename),
        new PropertyMetadata(default(Type), PropertyChangedCallback)
    );

    public Typename() {
        InitializeComponent();
        Refresh();
    }

    public Type? ValueType {
        get => (Type)GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((Typename)d).Refresh();
    }


    private void Refresh() {
        if (ValueType == null) {
            TypenameTextBlock.Text = "void";
            return;
        }

        TypenameTextBlock.Text = ValueType.Name;
    }
}