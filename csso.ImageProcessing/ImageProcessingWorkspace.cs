using System;
using System.Linq;
using csso.NodeCore;
using csso.NodeRunner.Shared;
using csso.OpenCL;
using OpenTK.Compute.OpenCL;
using Buffer = csso.OpenCL.Buffer;

namespace csso.ImageProcessing;

public class ImageProcessingWorkspace : IComputationContext, IDisposable {
    private readonly csso.OpenCL.Context _clContext;

    public ImageProcessingWorkspace() {
        _clContext = new();
    }

    public void RegisterFunctions(FunctionFactory graphFunctionFactory) {
        // throw new NotImplementedException();
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
        Program p = new(_clContext, code);
    }

    private void PngLoadTest_Button_OnClick() {
        Image img = new("C:\\1.png");
        var pixelCount = img.TotalPixels;

        var pixels8U = img.As<RGB8U>();
        var pixels16U = new RGB16U[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            pixels16U[i] = new RGB16U(pixels8U[i]);

        var resultPixels = new RGB16U[pixelCount];

        var a = Buffer.Create(_clContext, pixels16U);
        Buffer b = new(_clContext, sizeof(ushort) * 3 * pixelCount);

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

    public void Dispose() {
        _clContext.Dispose();
    }
}