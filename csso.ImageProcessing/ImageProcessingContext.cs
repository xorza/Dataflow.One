using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using csso.ImageProcessing.Funcs;
using csso.NodeCore;
using csso.NodeRunner.Shared;
using csso.OpenCL;

namespace csso.ImageProcessing;

public class ImageProcessingContext : IComputationContext, IDisposable {
    public ClContext ClContext { get; }
    public UiApi? UiApi { get; private set; }

    public ImageProcessingContext() {
        ClContext = new();
    }

    public void Init(UiApi api) {
        UiApi = api;
    }

    public void RegisterFunctions(FunctionFactory functionFactory) {
        functionFactory.Register(new FileImageSource(this));
        functionFactory.Register(new Function("Messagebox", Messagebox));
    }

    private void OpenCLTest2_Button_OnClick() {
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
        Program p = new(ClContext!, code);
    }

    private void PngLoadTest_Button_OnClick() {
        Image img = new("C:\\1.png");
        var pixelCount = img.TotalPixels;

        var pixels8U = img.As<RGB8U>();
        var pixels16U = new RGB16U[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            pixels16U[i] = new RGB16U(pixels8U[i]);

        var resultPixels = new RGB16U[pixelCount];

        var a = ClBuffer.Create(ClContext!, pixels16U);
        ClBuffer b = new(ClContext, sizeof(ushort) * 3 * pixelCount);

        var code = @"
                __kernel void add(__global ushort3* A, __global ushort3* B, const float C) {
                    int i = get_global_id(0);
					B[i] = A[i];
                }";
        Program program = new(ClContext, code);
        var kernel = program.Kernels.Single();
        CommandQueue commandQueue = new(ClContext);

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

    public void Dispose() {
        ClContext?.Dispose();
    }

    [Description("messagebox")]
    [FunctionId("18D7EE8B-F4F6-4C72-932D-80A47AF12012")]
    private bool Messagebox(Object message) {
        Debug.WriteLine(message.ToString() + " we34v5y245");
        return true;
    }
}