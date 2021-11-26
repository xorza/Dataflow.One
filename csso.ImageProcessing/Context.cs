using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using csso.Common;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.ES20;
using Debug = System.Diagnostics.Debug;

namespace csso.ImageProcessing {
public class Context : IDisposable {
    internal CLContext ClContext { get; }
    internal CLDevice[] ClDevices { get; }
    internal CLDevice SelectedClDevice { get; }

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

        Program program = new (this, code);
        
        CLResultCode result;
        // CLProgram program = CL.CreateProgramWithSource(ClContext, code, out result);
        // result.ValidateSuccess();

        // result = CL.BuildProgram(program, (uint) ClDevices.Length, ClDevices, null, IntPtr.Zero, IntPtr.Zero);
        // result.ValidateSuccess();

        Kernel kernel = program.Kernels.Single(_ => _.Name == "add");

        // CLKernel kernel = CL.CreateKernel(program, "add", out result);

        int arraySize = 20;
        float[] a = new float[arraySize];
        float[] b1 = new float[arraySize];
        float[] b2 = {1, 1, 1, 1};

        for (int i = 0; i < arraySize; i++) {
            a[i] = i;
            b1[i] = 1;
        }

        // CLBuffer bufferA = CL.CreateBuffer(ClContext,
        //     MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr,
        //     a,
        //     out result);
        // result.ValidateSuccess();
        // CLBuffer bufferB = CL.CreateBuffer(ClContext,
        //     MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr,
        //     b1,
        //     out result);
        // result.ValidateSuccess();

        Buffer buffer_a = Buffer.Create(this, a);
        Buffer buffer_b = Buffer.Create(this, b1);

        // CLBuffer resultBuffer = CL.CreateBuffer(
        //     ClContext,
        //     MemoryFlags.WriteOnly,
        //     new UIntPtr((uint) (arraySize * sizeof(float))),
        //     IntPtr.Zero,
        //     out result);
        // result.ValidateSuccess();

        Buffer result_buffer = new Buffer(this, arraySize * sizeof(float));
        
        CommandQueue commandQueue = new (this);

        try {
            // result = CL.SetKernelArg(kernel, 0, bufferA);
            // result.ValidateSuccess();
            // result = CL.SetKernelArg(kernel, 1, bufferB);
            // result.ValidateSuccess();
            // result = CL.SetKernelArg(kernel, 2, resultBuffer);
            // result.ValidateSuccess();
            // result = CL.SetKernelArg(kernel, 3, 1f);
            // result.ValidateSuccess();

            // commandQueue = CL.CreateCommandQueueWithProperties(
            //     ClContext,
            //     SelectedClDevice,
            //     IntPtr.Zero,
            //     out result);
            // result.ValidateSuccess();

           

            result = CL.EnqueueFillBuffer(
                commandQueue.ClCommandQueue,
                buffer_b.ClBuffer,
                b2,
                UIntPtr.Zero,
                (UIntPtr) (arraySize * sizeof(float)),
                null,
                out _);
            result.ValidateSuccess();

            result = CL.EnqueueNDRangeKernel(
                commandQueue.ClCommandQueue,
                kernel.ClKernel,
                1,
                null,
                new UIntPtr[] {new UIntPtr((uint) a.Length)},
                null,
                0,
                null,
                out _);
            result.ValidateSuccess();

            float[] resultValues = new float[arraySize];
            result = CL.EnqueueReadBuffer(
                commandQueue.ClCommandQueue,
                result_buffer.ClBuffer,
                true,
                UIntPtr.Zero,
                resultValues,
                null,
                out _);
            result.ValidateSuccess();

            CL.Finish(commandQueue.ClCommandQueue).ValidateSuccess();

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
            buffer_a.Dispose();
            buffer_b.Dispose();
            result_buffer.Dispose();
            commandQueue.Dispose();
            program.Dispose();
            kernel.Dispose();

            // CL.ReleaseProgram(program);
            // CL.ReleaseKernel(kernel);
        }
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