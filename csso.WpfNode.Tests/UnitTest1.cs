using System;
using System.IO;
using System.Text.Json;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.WpfNode.Tests;

public class Tests {
    private static Int32 _output;

    private string? _fileContent;

    [Description("messagebox")]
    private static bool Output(Int32 i) {
        _output = i;
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

        Function addFunc = new("Add", F.Add);
        Function divideWholeFunc = new("Divide whole", F.DivideWhole);
        Function messageBoxFunc = new("Output", Output);
        Function valueFunc = new("Value", Const);
        FrameNoFunc frameNoFunc = new();

        FunctionFactory functionFactory = new();
        functionFactory.Register(addFunc);
        functionFactory.Register(divideWholeFunc);
        functionFactory.Register(messageBoxFunc);
        functionFactory.Register(valueFunc);
        functionFactory.Register(frameNoFunc);

        SerializedGraphView? serializedGraphView = JsonSerializer.Deserialize<SerializedGraphView>(_fileContent!);
        Assert.NotNull(serializedGraphView);
        GraphView graphView = new(functionFactory, serializedGraphView.Value);

        Executor executor = new ( graphView.Graph);
        frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(12, _output);
        executor.Run();
        Assert.AreEqual(13, _output);

        String serialized = JsonSerializer.Serialize(graphView.Serialize(), opts);
        Assert.AreEqual(serialized, _fileContent);

        serializedGraphView = JsonSerializer.Deserialize<SerializedGraphView>(serialized);
        Assert.NotNull(serializedGraphView);
        graphView = new(functionFactory, serializedGraphView.Value);

        executor =  new ( graphView.Graph);
        frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(12, _output);
        executor.Run();
        Assert.AreEqual(13, _output);

        Assert.Pass();
    }
}