using dfo.NodeCore;

namespace dfo.NodeRunner.Shared;

public interface IComputationContext {
    void Init(UiApi api);
    void RegisterFunctions(FunctionFactory functionFactory);
    void OnStartRun();
    void OnFinishRun();
}

public class DummyComputationContext : IComputationContext {
    void IComputationContext.Init(UiApi api) { }
    void IComputationContext.RegisterFunctions(FunctionFactory functionFactory) { }
    void IComputationContext.OnStartRun() { }
    void IComputationContext.OnFinishRun() { }
}