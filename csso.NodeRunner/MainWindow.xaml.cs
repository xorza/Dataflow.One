using System.IO;
using System.Text.Json;
using System.Windows;
using csso.NodeRunner.PlayRoom;
using csso.WpfNode;
using Microsoft.Win32;

namespace csso.NodeRunner;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

         OverviewContentControl.Content = new Overview(new ScalarNodeRunner());
    }
}