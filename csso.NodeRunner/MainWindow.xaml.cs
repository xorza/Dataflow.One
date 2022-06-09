using System.IO;
using System.Text.Json;
using System.Windows;
using csso.WpfNode;
using Microsoft.Win32;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    private NodeRunner _nodeRunner;

    public MainWindow() {
        InitializeComponent();

        _nodeRunner = new NodeRunner();
        GraphOverview.Init(_nodeRunner);
    }

    private void Exit_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private void New_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        _nodeRunner = new NodeRunner();
        GraphOverview.Init(_nodeRunner);
    }

 }