using System;
using System.Collections.Generic;
using csso.NodeCore;

namespace csso.Nodeshop.Shared;

public interface IComputationContext {
    void Init(UiApi api);
    IEnumerable<Function> RegisterFunctions();
    void OnStartRun();
    void OnFinishRun();
}

public class DummyComputationContext : IComputationContext {
    void IComputationContext.Init(UiApi api) {
    }

    IEnumerable<Function> IComputationContext.RegisterFunctions() {
        return Array.Empty<Function>();
    }

    void IComputationContext.OnStartRun() {
    }

    void IComputationContext.OnFinishRun() {
    }
}