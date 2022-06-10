using csso.NodeCore;

namespace csso.NodeRunner.Shared;

public interface IComputationContext {
    void RegisterFunctions(FunctionFactory graphFunctionFactory);
}

public class DummyComputationContext : IComputationContext {
    void IComputationContext.RegisterFunctions(FunctionFactory graphFunctionFactory) {
        
    }
}