using System.IO;
using System.Text.Json;
using System.Windows;
using csso.WpfNode;
using Microsoft.Win32;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    private ScalarNodeRunner _scalarNodeRunner;

    public MainWindow() {
        InitializeComponent();

        _scalarNodeRunner = new ScalarNodeRunner();
        GraphOverview.Init(_scalarNodeRunner);
    }

    private void Exit_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        Close();
    }

    private void New_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        _scalarNodeRunner = new ScalarNodeRunner();
        GraphOverview.Init(_scalarNodeRunner);
    }

 }