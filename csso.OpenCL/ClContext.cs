using System;
using System.Linq;
using System.Text;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class ClContext : IDisposable {
    public ClContext() {
        IsDisposed = false;

        CL
            .GetPlatformIds(out var platformIds)
            .ValidateSuccess();

        foreach (var platform in platformIds) {
            CL
                .GetPlatformInfo(platform, PlatformInfo.Name, out var val)
                .ValidateSuccess();
        }

        foreach (var platform in platformIds) {
            CL
                .GetDeviceIds(platform, DeviceType.All, out var devices)
                .ValidateSuccess();

            var context = CL
                .CreateContext(IntPtr.Zero, devices, IntPtr.Zero,
                    IntPtr.Zero, out var result);
            result.ValidateSuccess();

            if (devices.Length == 0) {
                continue;
            }

            InternalCLContext = context;
            ClDevices = devices;
            SelectedClDevice = devices.First();
            return;
        }

        throw new InvalidOperationException("cannot create context");
    }

    internal CLContext InternalCLContext { get; }
    internal CLDevice[] ClDevices { get; }
    internal CLDevice SelectedClDevice { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    public string Test1() {
        CheckIfDisposed();

        var code = @"
                __kernel void add(__global float* A, __global float* B,__global float* result, const float C)
                {
                    int i = get_global_id(0);
                    result[i] = (A[i] + B[i]) + C;
					result[i] = (A[i] + B[i]);
                }";

        const int arraySize = 20;

        var a = new float[arraySize];
        var b = new float[arraySize];
        var resultValues = new float[arraySize];

        for (var i = 0; i < arraySize; i++) {
            a[i] = i;
            b[i] = 1;
        }

        ClProgram clProgram = new(this, code);
        var kernel = clProgram.Kernels.Single(_ => _.Name == "add");
        var bufferA = ClBuffer.Create(this, a);
        var bufferB = ClBuffer.Create(this, b);
        ClBuffer resultClBuffer = new(this, arraySize * sizeof(float));
        ClCommandQueue clCommandQueue = new(this);

        KernelArgValue[] argsValues = {
            new BufferKernelArgValue(bufferA),
            new BufferKernelArgValue(bufferB),
            new BufferKernelArgValue(resultClBuffer),
            new ScalarKernelArgValue<float>(1f)
        };

        try {
            clCommandQueue.EnqueueNdRangeKernel(kernel, arraySize, argsValues);
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

        return line.ToString();
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseContext(InternalCLContext);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    ~ClContext() {
        ReleaseUnmanagedResources();
    }
}