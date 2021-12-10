using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using csso.Calculator;
using csso.ImageProcessing;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.OpenCL;
using csso.WpfNode;
using Buffer = csso.OpenCL.Buffer;
using Graph = csso.NodeCore.Graph;
using Image = csso.ImageProcessing.Image;
using Node = csso.NodeCore.Node;

namespace WpfApp1; 

public partial class Overview : UserControl {
    public static readonly DependencyProperty GraphViewProperty = DependencyProperty.Register(
        "GraphView", typeof(GraphView), typeof(Overview), new PropertyMetadata(default(GraphView)));

    private readonly Context? _clContext;
    private readonly Executor _executor;
    private readonly Graph _graph;

    public Overview() {
        InitializeComponent();

        _graph = new Graph();
        _executor = new Executor(_graph);
        _clContext = new Context();

        IFunction addFunc = new Function("Add", F.Add);
        IFunction divideWholeFunc = new Function("Divide whole", F.DivideWhole);

        IFunction messageBoxFunc = new Function("Output", Output);
        IFunction valueFunc = new Function("Value", Const);


        _graph.Add(new Node(addFunc, _graph));

        _graph.Add(new Node(divideWholeFunc, _graph));
        _graph.Add(new Node(messageBoxFunc, _graph));

        _graph.Add(new Node(valueFunc, _graph));
        _graph.Add(new Node(valueFunc, _graph));
        _graph.Add(new Node(valueFunc, _graph));

        _graph.Add(new Node(_executor.FrameNoFunction, _graph));
        _graph.Add(new Node(_executor.DeltaTimeFunction, _graph));

        GraphView graphView = new(_graph);
        GraphView = graphView;

        DataContext = graphView;
        Graph.GraphView = graphView;
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
        _executor.Run();
    }

    private void ResetCtx_Button_OnClick(object sender, RoutedEventArgs args) {
        _executor.Reset();
    }
}