using System.IO;
using System.Windows;
using csso.NodeRunner.PlayRoom;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        OverviewContentControl.Content = new Overview(new ScalarNodeRunner());
    }
}