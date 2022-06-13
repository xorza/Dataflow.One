using System.Windows;
using csso.ImageProcessing;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        // OverviewContentControl.Content = new Overview(new ScalarComutationalContext());
        OverviewContentControl.Content = new Overview(new ImageProcessingContext());
    }
}