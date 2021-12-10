using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode;

public partial class Put : UserControl {
    public static readonly DependencyProperty PutViewProperty = DependencyProperty.Register(
        nameof(PutView), typeof(PutView), typeof(Put), new PropertyMetadata(default(PutView?)));

    public PutView? PutView {
        get { return (PutView?) GetValue(PutViewProperty); }
        set { SetValue(PutViewProperty, value); }
    }


    public Put() {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) { }

    private void PinHighlight_LoadedHandler(object sender, RoutedEventArgs args) {
        var element = (FrameworkElement) sender;
        PutView!.Control = element;
    }

    private void PinButton_Click(object sender, RoutedEventArgs e) {
        var pv = (PutView) ((Button) sender).Tag;
        PinClick?.Invoke(this,
            new PinClickEventArgs(pv) {
                RoutedEvent = e.RoutedEvent,
                Source = e.Source,
                Handled = false
            });
    }


    public event PinClickEventHandler? PinClick;
}