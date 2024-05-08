using System.Collections.Generic;
using System.ComponentModel;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.Nodeshop.Shared;

namespace csso.Nodeshop.PlayRoom;

public class ScalarComutationalContext : IComputationContext {
    public ConstantFunc<int> ConstIntFuncBaseBase { get; } = new("Integer");
    public ConstantFunc<double> ConstDoubleFuncBaseBase { get; } = new("Float");
    public ConstantFunc<string> ConstStringFuncBaseBase { get; } = new("String", "");

    public UiApi? UiApi { get; private set; }

    public void Init(UiApi api) {
        UiApi = api;
    }

    public IEnumerable<Function> RegisterFunctions() {
        var addFunc = new Function("Add", F.Add);
        var divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        var messageBoxFunc = new Function("Messagebox", Messagebox);

        yield return addFunc;
        yield return divideWholeFunc;
        yield return messageBoxFunc;
        yield return ConstIntFuncBaseBase;
        yield return ConstDoubleFuncBaseBase;
        yield return ConstStringFuncBaseBase;
    }

    public void OnStartRun() {
    }

    public void OnFinishRun() {
    }

    [Description("messagebox")]
    [FunctionId("A982AA64-D455-4EB5-8CE9-D7A75EDB00E5")]
    private bool Messagebox(object message) {
        UiApi!.ShowMessage(message?.ToString() ?? "null");
        return true;
    }
}