using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.WpfNode;
using Graph = csso.NodeCore.Graph;

namespace csso.NodeRunner;

public class Workspace {
    public Graph Graph { get; } = new();
    public FunctionFactory Factory { get; } = new();
    public FrameNoFunc FrameNoFunc { get; } = new();
    
    
    public Executor Executor { get; }
    public GraphView GraphView { get; }


    public Workspace() {
        
        Graph.FunctionFactory = Factory;
        

        Factory.Register(FrameNoFunc);
        FrameNoFunc.Executor = Executor;
        
        
        GraphView = new GraphView(Graph);
        
        Executor = new Executor(GraphView.Graph);
    }
}