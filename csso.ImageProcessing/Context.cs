using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using csso.Common;
using OpenTK.Compute.OpenCL;
using Debug = System.Diagnostics.Debug;

namespace csso.ImageProcessing {
public class Context : IDisposable {
    internal CLContext ClContext { get; }
    internal CLDevice[] ClDevices { get; }
    internal CLDevice SelectedDevice { get; }

    public Context() {
        IsDisposed = false;

        CLResultCode result;

        result = CL.GetPlatformIds(out CLPlatform[] platformIds);


        foreach (CLPlatform platform in platformIds) {
            result = CL.GetPlatformInfo(platform, PlatformInfo.Name, out byte[] val);
        }

        foreach (CLPlatform platform in platformIds) {
            result = CL.GetDeviceIds(platform, DeviceType.All, out CLDevice[] devices);

            CLContext context = CL.CreateContext(IntPtr.Zero, devices, IntPtr.Zero,
                IntPtr.Zero, out result);
            if (result != CLResultCode.Success) {
                throw new Exception("The context couldn't be created.");
            }

            if (devices.Length != 0) {
                ClContext = context;
                ClDevices = devices;
                SelectedDevice = devices.First();
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

        CLResultCode result;
        CLProgram program = CL.CreateProgramWithSource(ClContext, code, out result);
        result.ValidateSuccess();

        result = CL.BuildProgram(program, (uint) ClDevices.Length, ClDevices, null, IntPtr.Zero, IntPtr.Zero);
        result.ValidateSuccess();

        CLKernel kernel = CL.CreateKernel(program, "add", out result);

        int arraySize = 20;
        float[] a = new float[arraySize];
        float[] b1 = new float[arraySize];
        float[] b2 = new float[] {1, 1, 1, 1};

        for (int i = 0; i < arraySize; i++) {
            a[i] = i;
            b1[i] = 1;
        }

        CLBuffer bufferA = CL.CreateBuffer(ClContext, MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr, a,
            out result);
        CLBuffer bufferB = CL.CreateBuffer(ClContext, MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr, b1,
            out result);

        CLBuffer resultBuffer = new CLBuffer(CL.CreateBuffer(ClContext, MemoryFlags.WriteOnly,
            new UIntPtr((uint) (arraySize * sizeof(float))), IntPtr.Zero, out result));

        CLEvent eventHandle = default;
        CLCommandQueue commandQueue = default;

        try {
            result = CL.SetKernelArg(kernel, 0, bufferA);
            result.ValidateSuccess();
            result = CL.SetKernelArg(kernel, 1, bufferB);
            result.ValidateSuccess();
            result = CL.SetKernelArg(kernel, 2, resultBuffer);
            result.ValidateSuccess();
            result = CL.SetKernelArg(kernel, 3, 1f);
            result.ValidateSuccess();

            commandQueue = CL.CreateCommandQueueWithProperties(ClContext, SelectedDevice, IntPtr.Zero, out result);
            result.ValidateSuccess();

            result = CL.EnqueueFillBuffer(commandQueue, bufferB, b2, UIntPtr.Zero,
                (UIntPtr) (arraySize * sizeof(float)), null,
                out _);
            result.ValidateSuccess();

            result = CL.EnqueueNDRangeKernel(commandQueue, kernel, 1, null,
                new UIntPtr[] {new UIntPtr((uint) a.Length)},
                null, 0, null, out eventHandle);
            result.ValidateSuccess();

            float[] resultValues = new float[arraySize];
            result = CL.EnqueueReadBuffer(commandQueue, resultBuffer, true, UIntPtr.Zero, resultValues, null, out _);
            result.ValidateSuccess();

            CL.Finish(commandQueue).ValidateSuccess();

            StringBuilder line = new();
            foreach (float res in resultValues) {
                line.Append(res);
                line.Append(", ");
            }

            return line.ToString();
        }
        catch (Exception e) {
            Debug.WriteLine(e.ToString());
            throw;
        }
        finally {
            CL.ReleaseMemoryObject(bufferA);
            CL.ReleaseMemoryObject(bufferB);
            CL.ReleaseMemoryObject(resultBuffer);

            CL.ReleaseProgram(program);
            CL.ReleaseKernel(kernel);
            CL.ReleaseCommandQueue(commandQueue);
            CL.ReleaseEvent(eventHandle);
        }
    }

    public bool IsDisposed { get; }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseContext(ClContext);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed) {
            throw new InvalidOperationException("Context is disposed.");
        }
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Context() {
        ReleaseUnmanagedResources();
    }
}
}