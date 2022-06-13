using System;
using System.ComponentModel;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeRunner.Shared;

namespace csso.NodeRunner.PlayRoom;

public class ScalarComutationalContext : IComputationContext {
    public ScalarComutationalContext() { }

    public ConstantFunc<Int32> ConstIntFuncBaseBase { get; } = new("Integer");
    public ConstantFunc<Double> ConstDoubleFuncBaseBase { get; } = new("Float");
    public ConstantFunc<String> ConstStringFuncBaseBase { get; } = new("String", "");

    public UiApi? UiApi { get; private set; }

    [Description("messagebox")]
    [FunctionId("A982AA64-D455-4EB5-8CE9-D7A75EDB00E5")]
    private bool Messagebox(Object message) {
        UiApi!.ShowMessage(message?.ToString() ?? "null");
        return true;
    }

    public void Init(UiApi api) {
        UiApi = api;
    }

    public void RegisterFunctions(FunctionFactory functionFactory) {
        var addFunc = new Function("Add", F.Add);
        var divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        var messageBoxFunc = new Function("Messagebox", Messagebox);

        functionFactory.Register(addFunc);
        functionFactory.Register(divideWholeFunc);
        functionFactory.Register(messageBoxFunc);
        functionFactory.Register(ConstIntFuncBaseBase);
        functionFactory.Register(ConstDoubleFuncBaseBase);
        functionFactory.Register(ConstStringFuncBaseBase);
    }

    public void OnStartRun() {
        
    }

    public void OnFinishRun() {
    }
}