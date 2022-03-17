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

    private void Open_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        var ofd = new OpenFileDialog();
        ofd.Filter = "Json files | *.json";
        ofd.DefaultExt = "json";
        if (ofd.ShowDialog() ?? false) {
            JsonSerializerOptions opts = new();
            opts.WriteIndented = true;

            var jsonString = File.ReadAllText(ofd.FileName);
            var serializedGraphView = JsonSerializer.Deserialize<SerializedGraphView>(jsonString);

            _nodeRunner.Deserialize(serializedGraphView);
            GraphOverview.Init(_nodeRunner);
        }
    }

    private void Save_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        var sfd = new SaveFileDialog();
        sfd.Filter = "Json files | *.json";
        sfd.DefaultExt = "json";
        if (sfd.ShowDialog() ?? false) {
            JsonSerializerOptions opts = new();
            opts.WriteIndented = true;

            var serializedGraphView = _nodeRunner.Serialize();

            var jsonString = JsonSerializer.Serialize(serializedGraphView, opts);
            File.WriteAllText(sfd.FileName, jsonString);
        }
    }
}