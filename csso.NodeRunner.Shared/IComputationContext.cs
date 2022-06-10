using csso.NodeCore;

namespace csso.NodeRunner.Shared;

public interface IComputationContext {
    void Init(UiApi api);
    void RegisterFunctions(FunctionFactory graphFunctionFactory);
}

public class DummyComputationContext : IComputationContext {
    void IComputationContext.Init(UiApi api) { }
    void IComputationContext.RegisterFunctions(FunctionFactory graphFunctionFactory) { }
}