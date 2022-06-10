using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.NodeRunner.Shared;

namespace csso.NodeRunner;

public class Workspace {
    public Graph Graph { get; } = new();
    public FrameNoFunc FrameNoFunc { get; } = new();
    public Executor Executor { get; }

    public IComputationContext ComputationContext { get; }


    public Workspace(IComputationContext computationContext) {
        Graph.FunctionFactory.Register(FrameNoFunc);
        Executor = new Executor(Graph);
        FrameNoFunc.Executor = Executor;

        ComputationContext = computationContext;
        ComputationContext.RegisterFunctions(Graph.FunctionFactory);
    }
}