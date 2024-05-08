using System.Windows;
using csso.Nodeshop.PlayRoom;

namespace csso.Nodeshop;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        OverviewContentControl.Content = new Overview(new ScalarComutationalContext());
        // OverviewContentControl.Content = new Overview(new ImageProcessingContext());
    }
}