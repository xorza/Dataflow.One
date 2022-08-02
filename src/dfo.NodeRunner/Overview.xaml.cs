using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using dfo.NodeCore;
using dfo.NodeRunner.Shared;
using dfo.NodeRunner.UI;

namespace dfo.NodeRunner;

public partial class Overview : INotifyPropertyChanged {
    private FunctionFactoryBrowser? _functionFactoryBrowser;

    public Overview() : this(new DummyComputationContext()) { }

    public Overview(IComputationContext computationContext) {
        Workspace = new Workspace(computationContext);
        GraphView = new GraphView(Workspace.Graph);

        InitializeComponent();

        computationContext.Init(new UiApi());
    }

    public Workspace Workspace { get; }
    public GraphView GraphView { get; }
    public FunctionFactoryView FunctionFactoryView { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void FunctionFactoryBrowser_OnFunctionChosen(object? sender, Function e) {
        GraphView.CreateNode(e);
    }

    private void Run_ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        try {
            Workspace.ComputationContext.OnStartRun();
            Workspace.Executor.Run();
        } catch (ArgumentMissingException ex) {
            Console.Error.WriteLine(ex.ToString());
        } finally {
            Workspace.ComputationContext.OnFinishRun();
        }

        GraphView.OnFinishRun(Workspace.Executor);
    }

    private void FunctionFactoryBrowser_OnLoaded(object sender, RoutedEventArgs e) {
        FunctionFactoryView.Sync(Workspace.FunctionFactory);
        _functionFactoryBrowser = (FunctionFactoryBrowser) sender;
        _functionFactoryBrowser.FunctionFactoryView = FunctionFactoryView;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}