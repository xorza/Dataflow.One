using System.IO;
using System.Text.Json;
using System.Windows;
using csso.NodeRunner.Shared;
using csso.WpfNode;
using Microsoft.Win32;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        GraphOverview.Init(new Workspace());
    }
}