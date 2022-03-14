using System;
using System.ComponentModel;
using System.Windows;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.WpfNode;
using Graph = csso.NodeCore.Graph;

namespace csso.NodeRunner; 

public partial class MainWindow : Window {
    private readonly NodeRunner _nodeRunner;
    
    public MainWindow() {
        _nodeRunner = new ();
        
        InitializeComponent();
        
       

         GraphOverview.Init(_nodeRunner);
    }

    private void Exit_MenuItem_OnClick(object sender, RoutedEventArgs e) {
        throw new System.NotImplementedException();
    }
    

}