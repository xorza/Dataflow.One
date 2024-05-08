using csso.Common;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.Nodeshop.Infrastructure;
using csso.Nodeshop.Shared;

namespace csso.Nodeshop;

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