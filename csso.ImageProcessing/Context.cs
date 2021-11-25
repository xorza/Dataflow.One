using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using OpenTK.Compute.OpenCL;

namespace csso.ImageProcessing {
public class Context : IDisposable {
    internal CLContext ClContext { get; }
    internal CLDevice[] ClDevices { get; }

    public static Context? Create() {
        CLResultCode result;

        result = CL.GetPlatformIds(out CLPlatform[] platformIds);


        foreach (CLPlatform platform in platformIds) {
            result = CL.GetPlatformInfo(platform, PlatformInfo.Name, out byte[] val);
            String name = System.Text.Encoding.Default.GetString(val);
        }

        foreach (CLPlatform platform in platformIds) {
            result = CL.GetDeviceIds(platform, DeviceType.All, out CLDevice[] devices);

            CLContext context = CL.CreateContext(IntPtr.Zero, devices, IntPtr.Zero,
                IntPtr.Zero, out result);
            if (result != CLResultCode.Success) {
                throw new Exception("The context couldn't be created.");
            }

            return new Context(context, devices);
        }

        return null;
    }

    private Context(CLContext clContext, CLDevice[] devices) {
        IsDisposed = false;

        ClContext = clContext;
        ClDevices = devices;
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

        result = CL.BuildProgram(program, (uint) ClDevices.Length, ClDevices, null, IntPtr.Zero, IntPtr.Zero);

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

        CLBuffer resultBuffer = new CLBuffer(CL.CreateBuffer(ClContext, MemoryFlags.ReadWrite,
            new UIntPtr((uint) (arraySize * sizeof(float))), IntPtr.Zero, out result));

        CLEvent eventHandle = default;
        CLCommandQueue commandQueue = default;

        try {
            result = CL.SetKernelArg(kernel, 0, bufferA);
            result = CL.SetKernelArg(kernel, 1, bufferB);
            result = CL.SetKernelArg(kernel, 2, resultBuffer);
            result = CL.SetKernelArg(kernel, 3, 1f);

            commandQueue = CL.CreateCommandQueueWithProperties(ClContext, ClDevices[0], IntPtr.Zero, out result);

            result = CL.EnqueueFillBuffer(commandQueue, bufferB, b2, UIntPtr.Zero, (UIntPtr) (arraySize * sizeof(float)), null,
                out _);

            result = CL.EnqueueNDRangeKernel(commandQueue, kernel, 1, null, new UIntPtr[] {new UIntPtr((uint) a.Length)},
                null, 0, null, out eventHandle);

            // result = CL.Finish(commandQueue);

            float[] resultValues = new float[arraySize];
            result =  CL.EnqueueReadBuffer(commandQueue, resultBuffer, true, UIntPtr.Zero, resultValues, null, out _);

            result = CL.Finish(commandQueue);
            
            StringBuilder line = new StringBuilder();
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