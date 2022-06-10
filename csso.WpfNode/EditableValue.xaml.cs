using System;
using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode;

public partial class EditableValue : UserControl {
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register
    (
        nameof(Value), typeof(EditableValueView), typeof(EditableValue),
        new PropertyMetadata(null, PropertyChangedCallback)
    );

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (EditableValue) d;
        control.ValueTextBox.IsEnabled = e.NewValue != null;
    }

    public EditableValueView? Value {
        get => (EditableValueView) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public EditableValue() {
        InitializeComponent();
    }
}