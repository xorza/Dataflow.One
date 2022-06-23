using System;
using System.Linq;
using System.Text;
using dfo.OpenCL;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NUnit.Framework;

namespace dfo.ImageProcessing.Tests;

public class ClContextTests {
    private readonly ClContext _clContext = new ClContext();
    
    [SetUp]
    public void Setup() { }

    [Test]
    public void two_buffers_and_const_sum() {
        var code = @"
                __kernel void add(__global float* A, __global float* B,__global float* result, const float C)
                {
                    int x = get_global_id(0);
                    int y = get_global_id(1);
                    int i = y * 2 + x;

                    result[i] = (A[i] + B[i]) + C;
                }";

        UInt32 arraySize = 20;
        var a = new float[arraySize];
        var b = new float[arraySize];
        var resultValues = new float[arraySize];

        for (var i = 0; i < arraySize; i++) {
            a[i] = i;
            b[i] = 19;
        }

        ClProgram clProgram = new(_clContext, code);
        var kernel = clProgram.Kernels.Single(_ => _.Name == "add");
        var bufferA = ClBuffer.Create(_clContext, a);
        var bufferB = ClBuffer.Create(_clContext, b);
        ClBuffer resultClBuffer = new(_clContext, arraySize * sizeof(float));
        ClCommandQueue clCommandQueue = new(_clContext);

        ClKernelArgValue[] argsValues = {
            new BufferClKernelArgValue(bufferA),
            new BufferClKernelArgValue(bufferB),
            new BufferClKernelArgValue(resultClBuffer),
            new ScalarClKernelArgValue<float>(1f)
        };

        try {
            clCommandQueue.EnqueueNdRangeKernel(kernel, new int[] {2,10}, argsValues);
            clCommandQueue.EnqueueReadBuffer(resultClBuffer, resultValues);
            clCommandQueue.Finish();
        }
        finally {
            bufferA.Dispose();
            bufferB.Dispose();
            resultClBuffer.Dispose();
            clCommandQueue.Dispose();
            clProgram.Dispose();
            kernel.Dispose();
        }

        StringBuilder line = new();
        foreach (var res in resultValues) {
            line.Append(res);
            line.Append(", ");
        }

        Assert.Pass();
    }
}
