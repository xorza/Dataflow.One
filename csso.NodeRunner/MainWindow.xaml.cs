using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.WpfNode;
using Microsoft.Win32;
using Graph = csso.NodeCore.Graph;

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

            string jsonString = File.ReadAllText(ofd.FileName);
            SerializedGraphView serializedGraphView = JsonSerializer.Deserialize<SerializedGraphView>(jsonString);

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

            SerializedGraphView serializedGraphView = _nodeRunner.Serialize();

            string jsonString = JsonSerializer.Serialize(serializedGraphView, opts);
            File.WriteAllText(sfd.FileName, jsonString);
        }
    }
}