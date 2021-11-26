using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using csso.Common;
using csso.OpenCL;
using OpenTK.Compute.OpenCL;
using Buffer = csso.OpenCL.Buffer;
using Debug = System.Diagnostics.Debug;

namespace csso.OpenCL {
public class Context : IDisposable {
    internal CLContext ClContext { get; }
    internal CLDevice[] ClDevices { get; }
    internal CLDevice SelectedClDevice { get; }

    public Context() {
        IsDisposed = false;

        CL.GetPlatformIds(out CLPlatform[] platformIds)
            .ValidateSuccess();

        foreach (CLPlatform platform in platformIds)
            CL.GetPlatformInfo(platform, PlatformInfo.Name, out byte[] val)
                .ValidateSuccess();

        foreach (CLPlatform platform in platformIds) {
            CL.GetDeviceIds(platform, DeviceType.All, out CLDevice[] devices)
                .ValidateSuccess();

            CLContext context = CL.CreateContext(IntPtr.Zero, devices, IntPtr.Zero,
                IntPtr.Zero, out CLResultCode result);
            result.ValidateSuccess();

            if (devices.Length != 0) {
                ClContext = context;
                ClDevices = devices;
                SelectedClDevice = devices.First();
                return;
            }
        }

        throw new InvalidOperationException("cannot create context");
    }


    public String Test1() {
        CheckIfDisposed();

        string code = @"
                __kernel void add(__global float* A, __global float* B,__global float* result, const float C)
                {
                    int i = get_global_id(0);
                    result[i] = (A[i] + B[i]) + C;
					result[i] = (A[i] + B[i]);
                }";

        const Int32 arraySize = 20;

        float[] a = new float[arraySize];
        float[] b = new float[arraySize];
        float[] resultValues = new float[arraySize];

        for (Int32 i = 0; i < arraySize; i++) {
            a[i] = i;
            b[i] = 1;
        }

        Program program = new(this, code);
        Kernel kernel = program.Kernels.Single(_ => _.Name == "add");
        Buffer bufferA = Buffer.Create(this, a);
        Buffer bufferB = Buffer.Create(this, b);
        Buffer resultBuffer = new(this, arraySize * sizeof(float));
        CommandQueue commandQueue = new(this);

        KernelArgValue[] argsValues = {
            new BufferKernelArgValue(bufferA),
            new BufferKernelArgValue(bufferB),
            new BufferKernelArgValue(resultBuffer),
            new ScalarKernelArgValue<float>(1f)
        };

        try {
            commandQueue.EnqueueNdRangeKernel(kernel, arraySize, argsValues);
            commandQueue.EnqueueReadBuffer(resultBuffer, resultValues);
            commandQueue.Finish();
        }
        finally {
            bufferA.Dispose();
            bufferB.Dispose();
            resultBuffer.Dispose();
            commandQueue.Dispose();
            program.Dispose();
            kernel.Dispose();
        }

        StringBuilder line = new();
        foreach (float res in resultValues) {
            line.Append(res);
            line.Append(", ");
        }

        return line.ToString();
    }

    public bool IsDisposed { get; private set; } = false;

    private void ReleaseUnmanagedResources() {
        CL.ReleaseContext(ClContext);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Context() {
        ReleaseUnmanagedResources();
    }
}
}