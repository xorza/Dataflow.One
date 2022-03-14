using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using csso.Common;
using csso.ImageProcessing;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using csso.OpenCL;
using csso.WpfNode;
using Microsoft.Win32;
using Buffer = csso.OpenCL.Buffer;
using Executor = csso.NodeCore.Run.Executor;
using Graph = csso.NodeCore.Graph;
using Image = csso.ImageProcessing.Image;
using Node = csso.NodeCore.Node;

namespace csso.NodeRunner;

public partial class Overview {
    public static readonly DependencyProperty GraphViewProperty = DependencyProperty.Register(
        nameof(GraphView), typeof(GraphVM), typeof(Overview), new PropertyMetadata(default(GraphVM)));


    private readonly Context _clContext;
    private Executor? _executor;

    public GraphVM? GraphView {
        get => (GraphVM) GetValue(GraphViewProperty);
        private set => SetValue(GraphViewProperty, value);
    }
    
    public Overview() {
        InitializeComponent();

        _clContext = new Context();
    }

    public void Init(NodeRunner nodeRunner) {
        GraphView = nodeRunner.GraphVM;
        _executor = nodeRunner.Executor;
    }

    private void DetectCycles_Button_OnClick(object sender, RoutedEventArgs e) {
        NoLoopValidator validator = new();
        validator.Go(GraphView!.Graph);
    }

    private void OpenCLTest1_Button_OnClick(object sender, RoutedEventArgs e) {
        var result = _clContext.Test1();
        MessageBox.Show(result, "OpenCL test results");
    }

    private void OpenCLTest2_Button_OnClick(object sender, RoutedEventArgs e) {
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
        Image img = new("C:\\1.png");
        var pixelCount = img.TotalPixels;

        var pixels8U = img.As<RGB8U>();
        var pixels16U = new RGB16U[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            pixels16U[i] = new RGB16U(pixels8U[i]);

        var resultPixels = new RGB16U[pixelCount];

        var a = Buffer.Create(_clContext!, pixels16U);
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
    
    private void FunctionFactoryBrowser_OnFunctionChosen(object? sender, Function e) {
        GraphView!.CreateNode(e);
    }

    private void Run_ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        try {
            _executor!.Run();
        } catch (ArgumentMissingException) {
            
        }
        
        GraphView!.OnExecuted(_executor!);
    }
}