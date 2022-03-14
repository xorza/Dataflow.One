using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
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
    public Executor Executor { get; private set; }
    public GraphVM GraphVM { get; set; }

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

        GraphVM = new GraphVM(graph);
        Executor = new(GraphVM.Graph);
        FrameNoFunc.Executor = Executor;
    }

    public void Deserialize(SerializedGraphView serialized) {
        GraphVM = new GraphVM(Factory, serialized);
        Executor = new(GraphVM.Graph);
        FrameNoFunc.Executor = Executor;
    }

    public SerializedGraphView Serialize() {
        return GraphVM.Serialize();
    }


    [Description("messagebox")]
    [FunctionId("A982AA64-D455-4EB5-8CE9-D7A75EDB00E5")]
    private static bool Output(object i) {
        MessageBox.Show(i.ToString());
        return true;
    }

    [Description("value")]
    [Reactive]
    [FunctionId("28005F51-BD05-4871-BD0B-AA23C2ADCB9C")]
    private static bool Const([Config(12)] Int32 c, [Output] ref Int32 i) {
        i = c;
        return true;
    }
}