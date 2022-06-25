using System;
using System.Collections.Generic;
using dfo.NodeCore;

namespace dfo.NodeRunner.Shared;

public interface IComputationContext {
    void Init(UiApi api);
    IEnumerable<Function> RegisterFunctions();
    void OnStartRun();
    void OnFinishRun();
}

public class DummyComputationContext : IComputationContext {
    void IComputationContext.Init(UiApi api) { }

    IEnumerable<Function> IComputationContext.RegisterFunctions() {
        return Array.Empty<Function>();
    }

    void IComputationContext.OnStartRun() { }
    void IComputationContext.OnFinishRun() { }
}