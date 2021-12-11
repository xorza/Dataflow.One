using System;
using System.IO;
using System.Text.Json;
using csso.Calculator;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using NUnit.Framework;

namespace csso.WpfNode.Tests;

public class Tests {
    private static Int32 output;

    private string? _fileContent;

    [Description("messagebox")]
    private static bool Output(Int32 i) {
        output = i;
        return true;
    }

    [Description("value")]
    [Reactive]
    private static bool Const([Config(12)] Int32 c, [Output] ref Int32 i) {
        i = c;
        return true;
    }

    [SetUp]
    public void Setup() {
        _fileContent = File.ReadAllText("graph1.json");
    }

    [Test]
    public void SameAfterSerialization() {
        Assert.NotNull(_fileContent);
        
        JsonSerializerOptions opts = new();
        opts.WriteIndented = true;
        
        Function addFunc = new ("Add", F.Add);
        Function divideWholeFunc = new ("Divide whole", F.DivideWhole);
        Function messageBoxFunc = new ("Output", Output);
        Function valueFunc = new ("Value", Const);

        Executor executor = new();

        FunctionFactory functionFactory = new();
        functionFactory.Register(addFunc);
        functionFactory.Register(divideWholeFunc);
        functionFactory.Register(messageBoxFunc);
        functionFactory.Register(valueFunc);
        functionFactory.Register(executor.FrameNoFunction);
        functionFactory.Register(executor.DeltaTimeFunction);
        
        SerializedGraphView? serializedGraphView = JsonSerializer.Deserialize<SerializedGraphView>(_fileContent!);
        Assert.NotNull(serializedGraphView);
        GraphView graphView = new(functionFactory, serializedGraphView.Value);
        
        executor.Run(graphView.Graph);
        Assert.AreEqual(12, output);  
        executor.Run(graphView.Graph);
        Assert.AreEqual(13, output);
        
        String serialized = JsonSerializer.Serialize(graphView.Serialize(), opts);
        
        Assert.AreEqual(serialized, _fileContent);

        Assert.Pass();
    }
    
}