using System;
using System.ComponentModel;
using System.Windows;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.WpfNode;
using Graph = csso.NodeCore.Graph;

namespace csso.NodeRunner;

public class ScalarNodeRunner: Workspace {
    public ScalarNodeRunner()  {

        var addFunc = new Function("Add", F.Add);
        var divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        var messageBoxFunc = new Function("Messagebox", Output);

        Factory.Register(addFunc);
        Factory.Register(divideWholeFunc);
        Factory.Register(messageBoxFunc);
        Factory.Register(ConstIntFunc);
        Factory.Register(ConstDoubleFunc);
        Factory.Register(ConstStringFunc);
        
        GraphView.Sync();
    }

    public ConstantFunc<Int32> ConstIntFunc { get; } = new("Integer");
    public ConstantFunc<Double> ConstDoubleFunc { get; } = new("Float");
    public ConstantFunc<String> ConstStringFunc { get; } = new("String", "");

    [Description("messagebox")]
    [FunctionId("A982AA64-D455-4EB5-8CE9-D7A75EDB00E5")]
    private static bool Output(Object message) {
        MessageBox.Show(message.ToString());
        return true;
    }
}