using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.Common;
using csso.ImageProcessing;
using csso.NodeCore;
using csso.NodeCore.Run;
using csso.NodeRunner.Shared;
using csso.NodeRunner.UI;

namespace csso.NodeRunner;

public partial class Overview : INotifyPropertyChanged {
    private FunctionFactoryBrowser? _functionFactoryBrowser;

    public Workspace Workspace { get; }
    public GraphView GraphView { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public Overview() : this(new DummyComputationContext()) { }
    
    public Overview(IComputationContext computationContext) {
        Workspace = new Workspace(computationContext);
        GraphView = new GraphView(Workspace.Graph);
        
        InitializeComponent();
        
        computationContext.Init(new UiApi());
    }

    private void FunctionFactoryBrowser_OnFunctionChosen(object? sender, Function e) {
        GraphView.CreateNode(e);
    }
    private void Run_ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        try {
            Workspace.ComputationContext.OnStartRun();
            Workspace.Executor.Run();
        }
        catch (ArgumentMissingException ex) {
            Console.Error.WriteLine(ex.ToString());
        }
        finally {
            Workspace.ComputationContext.OnFinishRun();
        }

        GraphView.OnFinishRun(Workspace.Executor);
    }
    private void FunctionFactoryBrowser_OnLoaded(object sender, RoutedEventArgs e) {
        _functionFactoryBrowser = (FunctionFactoryBrowser) sender;
        _functionFactoryBrowser.FunctionFactory = GraphView.FunctionFactory;
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}