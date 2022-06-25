using dfo.Common;
using dfo.NodeCore;
using dfo.NodeCore.Funcs;
using dfo.NodeCore.Run;
using dfo.NodeRunner.Infrastructure;
using dfo.NodeRunner.Shared;

namespace dfo.NodeRunner;

public class Workspace {
    public Workspace(IComputationContext computationContext) {
        Executor = new Executor(Graph);
        FrameNoFunc.Executor = Executor;

        ComputationContext = computationContext;
        var functions = ComputationContext.RegisterFunctions();

        functions.ForEach(FunctionFactory.Register);
        FunctionFactory.Register(FrameNoFunc);
    }

    public Graph Graph { get; } = new();
    public FrameNoFunc FrameNoFunc { get; } = new();
    public Executor Executor { get; }
    public FunctionFactory FunctionFactory { get; } = new();

    public IComputationContext ComputationContext { get; }
}