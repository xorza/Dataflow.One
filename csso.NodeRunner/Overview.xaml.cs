using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.ImageProcessing;
using csso.NodeCore;
using csso.NodeCore.Run;
using csso.OpenCL;
using csso.WpfNode;

namespace csso.NodeRunner;

public partial class Overview : INotifyPropertyChanged {
    private readonly Context _clContext;
    private Executor? _executor;

    private FunctionFactoryBrowser? _functionFactoryBrowser;

    private GraphView? _graphView;


    public Overview() {
        InitializeComponent();

        _clContext = new Context();
    }

    public GraphView? GraphView {
        get => _graphView;
        set {
            _graphView = value;
            Graph.GraphView = _graphView;
            if (_functionFactoryBrowser != null) {
                _functionFactoryBrowser.FunctionFactory = _graphView?.FunctionFactory;
            }
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Init(Workspace scalarNodeRunner) {
        GraphView = scalarNodeRunner.GraphView;
        _executor = scalarNodeRunner.Executor;
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
        _graphView!.CreateNode(e);
    }

    private void Run_ButtonBase_OnClick(object sender, RoutedEventArgs e) {
        try {
            _executor!.Run();
        } catch (ArgumentMissingException) { }

        _graphView!.OnExecuted(_executor!);
    }

    private void FunctionFactoryBrowser_OnLoaded(object sender, RoutedEventArgs e) {
        _functionFactoryBrowser = (FunctionFactoryBrowser) sender;

        _functionFactoryBrowser.FunctionFactory = _graphView?.FunctionFactory;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}