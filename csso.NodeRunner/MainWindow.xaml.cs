using System.IO;
using System.Text.Json;
using System.Windows;
using csso.WpfNode;
using Microsoft.Win32;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    private Workspace _workspace;

    public MainWindow() {
        InitializeComponent();

        _workspace = new Workspace();
        GraphOverview.Init(_workspace);
    }

    private void Exit_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private void New_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        _workspace = new ScalarNodeRunner();
        GraphOverview.Init(_workspace);
    }

 }