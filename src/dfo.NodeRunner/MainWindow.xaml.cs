using System.Windows;
using dfo.ImageProcessing;

namespace dfo.NodeRunner;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        // OverviewContentControl.Content = new Overview(new ScalarComutationalContext());
        OverviewContentControl.Content = new Overview(new ImageProcessingContext());
    }
}