using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using csso.Calculator;
using csso.ImageProcessing;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.OpenCL;
using csso.WpfNode;
using Microsoft.Win32;
using Buffer = csso.OpenCL.Buffer;
using Graph = csso.NodeCore.Graph;
using Image = csso.ImageProcessing.Image;
using Node = csso.NodeCore.Node;

namespace csso.NodeRunner;

public partial class Overview {
    public static readonly DependencyProperty GraphViewProperty = DependencyProperty.Register(
        nameof(GraphView), typeof(GraphView), typeof(Overview), new PropertyMetadata(default(GraphView)));

    private readonly Context? _clContext;
    private readonly Executor _executor = new();
    private Graph _graph;

    private readonly FunctionFactory _functionFactory = new();

    public Overview() {
        InitializeComponent();

        _graph = new Graph();
        _clContext = new Context();

        Function addFunc = new Function("Add", F.Add);
        Function divideWholeFunc = new Function("Divide whole", F.DivideWhole);
        Function messageBoxFunc = new Function("Output", Output);
        Function valueFunc = new Function("Value", Const);

        _functionFactory.Register(addFunc);
        _functionFactory.Register(divideWholeFunc);
        _functionFactory.Register(messageBoxFunc);
        _functionFactory.Register(valueFunc);
        _functionFactory.Register(_executor.FrameNoFunction);
        _functionFactory.Register(_executor.DeltaTimeFunction);

        _graph.FunctionFactory = _functionFactory;

        // _graph.Add(new Node(addFunc, _graph));
        // _graph.Add(new Node(divideWholeFunc, _graph));
        // _graph.Add(new Node(messageBoxFunc, _graph));
        // _graph.Add(new Node(valueFunc, _graph));
        // _graph.Add(new Node(_executor.FrameNoFunction, _graph));
        // _graph.Add(new Node(valueFunc, _graph));
        // _graph.Add(new Node(valueFunc, _graph));
        // _graph.Add(new Node(_executor.DeltaTimeFunction, _graph));

        GraphView = new(_graph);
    }

    public GraphView GraphView {
        get => (GraphView) GetValue(GraphViewProperty);
        set => SetValue(GraphViewProperty, value);
    }

    [Description("messagebox")]
    private static bool Output(object i) {
        MessageBox.Show(i.ToString());
        return true;
    }

    [Description("value")]
    [Reactive]
    private static bool Const([Config(12)] Int32 c, [Output] ref Int32 i) {
        i = c;
        return true;
    }

    private void DetectCycles_Button_OnClick(object sender, RoutedEventArgs e) {
        NoLoopValidator validator = new();
        validator.Go(_graph);
    }

    private void OpenCLTest1_Button_OnClick(object sender, RoutedEventArgs e) {
        if (_clContext != null) {
            var result = _clContext.Test1();
            MessageBox.Show(result, "OpenCL test results");
        }
    }

    private void OpenCLTest2_Button_OnClick(object sender, RoutedEventArgs e) {
        if (_clContext == null) return;

        var code = @"
                __kernel void add(__global float* A, __global float* B,__global float* result, const float C)
                {
                    int i = get_global_id(0);
                    result[i] = (A[i] + B[i]) + C;
					result[i] = (A[i] + B[i]);
                }

                __kernel void test_test(__global float3* A, __global float* B,__global float* result, const float C)
                {
                    int i = get_global_id(0);
                    result[i] = (A[i].x + B[i]) + C;
					result[i] = (A[i].x + B[i]);
                }";
        Program p = new(_clContext!, code);
    }

    private void PngLoadTest_Button_OnClick(object sender, RoutedEventArgs e) {
        if (_clContext == null)
            return;

        Image img = new("C:\\1.png");
        var pixelCount = img.TotalPixels;

        var pixels8u = img.As<RGB8U>();
        var pixels16u = new RGB16U[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            pixels16u[i] = new RGB16U(pixels8u[i]);

        var resultPixels = new RGB16U[pixelCount];

        var a = Buffer.Create(_clContext!, pixels16u);
        Buffer b = new(_clContext!, sizeof(ushort) * 3 * pixelCount);

        var code = @"
                __kernel void add(__global ushort3* A, __global ushort3* B, const float C) {
                    int i = get_global_id(0);
					B[i] = A[i];
                }";
        Program program = new(_clContext, code);
        var kernel = program.Kernels.Single();
        CommandQueue commandQueue = new(_clContext);

        KernelArgValue[] argsValues = {
            new BufferKernelArgValue(a),
            new BufferKernelArgValue(b),
            new ScalarKernelArgValue<float>(1f)
        };

        try {
            commandQueue.EnqueueNdRangeKernel(kernel, pixelCount, argsValues);
            commandQueue.EnqueueReadBuffer(b, resultPixels);
            commandQueue.Finish();
        } finally {
            a.Dispose();
            b.Dispose();
            commandQueue.Dispose();
            program.Dispose();
            kernel.Dispose();
        }
    }

    private void RunGraph_Button_OnClick(object sender, RoutedEventArgs args) {
        _executor.Run(_graph);
    }

    private void ResetCtx_Button_OnClick(object sender, RoutedEventArgs args) {
        _executor.Reset();
    }

    private void Serialize_Button_OnClick(object sender, RoutedEventArgs e) {
        var sfd = new SaveFileDialog();
        sfd.Filter = "Json files | *.json";
        sfd.DefaultExt = "json";
        if (sfd.ShowDialog() ?? false) {
            JsonSerializerOptions opts = new();
            opts.WriteIndented = true;

            SerializedGraphView serializedGraphView = GraphView.Serialize();

            string jsonString = JsonSerializer.Serialize(serializedGraphView, opts);
            File.WriteAllText(sfd.FileName, jsonString);
        }
    }

    private void Deserialize_Button_OnClick(object sender, RoutedEventArgs e) {
        var ofd = new OpenFileDialog();
        ofd.Filter = "Json files | *.json";
        ofd.DefaultExt = "json";
        if (ofd.ShowDialog() ?? false) {
            JsonSerializerOptions opts = new();
            opts.WriteIndented = true;

            string jsonString = File.ReadAllText(ofd.FileName);
            SerializedGraphView serializedGraphView = JsonSerializer.Deserialize<SerializedGraphView>(jsonString);
            GraphView = new(_functionFactory, serializedGraphView);
            _graph = GraphView.Graph;
        }
    }

    private void FunctionFactoryBrowser_OnFunctionChosen(object? sender, Function e) {
        GraphView.CreateNode(e);
    }
}