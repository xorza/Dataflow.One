using System.IO;
using csso.ImageProcessing;
using csso.NodeRunner.PlayRoom;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = System.Windows.Window;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        OverviewContentControl.Content = new Overview(new ImageProcessingWorkspace());
    }
}