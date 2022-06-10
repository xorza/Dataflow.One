using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;

namespace csso.NodeRunner.Shared;

public class Workspace {
    public Graph Graph { get; } = new();
    public FrameNoFunc FrameNoFunc { get; } = new();
    public Executor Executor { get; }


    public Workspace() {
        Graph.FunctionFactory.Register(FrameNoFunc);
        Executor = new Executor(Graph);
        
        FrameNoFunc.Executor = Executor;
    }
}