using System.ComponentModel;
using csso.NodeCore;
using csso.NodeCore.Funcs;

namespace csso.NodeRunner.Shared;

public class ScalarNodeRunner : Workspace {
    public ScalarNodeRunner() {
        var addFunc = new Function("Add", F.Add);
        var divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        var messageBoxFunc = new Function("Messagebox", Messagebox);

        Graph.FunctionFactory.Register(addFunc);
        Graph.FunctionFactory.Register(divideWholeFunc);
        Graph.FunctionFactory.Register(messageBoxFunc);
        Graph.FunctionFactory.Register(ConstIntFunc);
        Graph.FunctionFactory.Register(ConstDoubleFunc);
        Graph.FunctionFactory.Register(ConstStringFunc);
    }

    public ConstantFunc<Int32> ConstIntFunc { get; } = new("Integer");
    public ConstantFunc<Double> ConstDoubleFunc { get; } = new("Float");
    public ConstantFunc<String> ConstStringFunc { get; } = new("String", "");

    [Description("messagebox")]
    [FunctionId("A982AA64-D455-4EB5-8CE9-D7A75EDB00E5")]
    private static bool Messagebox(Object message) {
        // System.Windows.MessageBox.Show(message.ToString());
        return true;
    }
}