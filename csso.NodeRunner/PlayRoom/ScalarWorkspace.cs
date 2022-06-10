using System;
using System.ComponentModel;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeRunner.Shared;

namespace csso.NodeRunner.PlayRoom;

public class ScalarNodeRunner : IComputationContext {
    public ScalarNodeRunner() { }

    public ConstantFunc<Int32> ConstIntFunc { get; } = new("Integer");
    public ConstantFunc<Double> ConstDoubleFunc { get; } = new("Float");
    public ConstantFunc<String> ConstStringFunc { get; } = new("String", "");

    [Description("messagebox")]
    [FunctionId("A982AA64-D455-4EB5-8CE9-D7A75EDB00E5")]
    private static bool Messagebox(Object message) {
        System.Windows.MessageBox.Show(message.ToString());
        return true;
    }

    public void RegisterFunctions(FunctionFactory functionFactory) {
        var addFunc = new Function("Add", F.Add);
        var divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        var messageBoxFunc = new Function("Messagebox", Messagebox);

        functionFactory.Register(addFunc);
        functionFactory.Register(divideWholeFunc);
        functionFactory.Register(messageBoxFunc);
        functionFactory.Register(ConstIntFunc);
        functionFactory.Register(ConstDoubleFunc);
        functionFactory.Register(ConstStringFunc);
    }
}