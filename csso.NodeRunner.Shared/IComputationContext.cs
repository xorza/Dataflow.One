using csso.NodeCore;

namespace csso.NodeRunner.Shared;

public interface IComputationContext {
    void Init(UiApi api);
    void RegisterFunctions(FunctionFactory functionFactory);
}

public class DummyComputationContext : IComputationContext {
    void IComputationContext.Init(UiApi api) { }
    void IComputationContext.RegisterFunctions(FunctionFactory functionFactory) { }
}