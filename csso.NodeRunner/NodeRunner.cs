using System;
using System.ComponentModel;
using System.Windows;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.WpfNode;
using Graph = csso.NodeCore.Graph;

namespace csso.NodeRunner; 

public class NodeRunner {
    public FunctionFactory Factory { get; } = new();
    public FrameNoFunc FrameNoFunc { get; }
    public Executor Executor { get; }
    public GraphVM GraphVM { get; }

    public NodeRunner() {
        Graph graph = new();

        Function addFunc = new Function("Add", F.Add);
        Function divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        Function messageBoxFunc = new Function("Output", Output);
        Function valueFunc = new Function("Value", Const);
        FrameNoFunc = new();

        Factory.Register(addFunc);
        Factory.Register(divideWholeFunc);
        Factory.Register(messageBoxFunc);
        Factory.Register(valueFunc);
        Factory.Register(FrameNoFunc);

        graph.FunctionFactory = Factory;
        
        Executor = new(graph);
        FrameNoFunc.Executor = Executor;

        GraphVM = new(graph);
    }
    
    [Description("messagebox")]
    private static bool Output(object i) {
        MessageBox.Show(i.ToString());
        return true;
    }

    [Description("value")]
    [Reactive]
    private static bool Const([Config(12)] Int32 c, [Output] ref Int32 i) {
        i = c;
        return true;
    }
}