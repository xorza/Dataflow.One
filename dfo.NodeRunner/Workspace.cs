using dfo.NodeCore;
using dfo.NodeCore.Funcs;
using dfo.NodeCore.Run;
using dfo.NodeRunner.Shared;

namespace dfo.NodeRunner;

public class Workspace {
    public Workspace(IComputationContext computationContext) {
        Graph.FunctionFactory.Register(FrameNoFunc);
        Executor = new Executor(Graph);
        FrameNoFunc.Executor = Executor;

        ComputationContext = computationContext;
        ComputationContext.RegisterFunctions(Graph.FunctionFactory);
    }

    public Graph Graph { get; } = new();
    public FrameNoFunc FrameNoFunc { get; } = new();
    public Executor Executor { get; }

    public IComputationContext ComputationContext { get; }
}