using System;
using System.Collections.Generic;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
public class CommandQueue : IDisposable {
    public Context Context { get; }

    internal CLCommandQueue ClCommandQueue { get; }

    public CommandQueue(Context context) {
        context.CheckIfDisposed();

        Context = context;

        CLResultCode result;
        ClCommandQueue = CL.CreateCommandQueueWithProperties(
            context.ClContext,
            context.SelectedClDevice,
            IntPtr.Zero,
            out result);
        result.ValidateSuccess();
    }

    public void EnqueueFillBuffer<T>(Buffer buffer, T[] arr) where T : unmanaged {
        CheckIfDisposed();
        buffer.CheckIfDisposed();

        unsafe {
            CLResultCode result;
            CLEvent clEvent;
            result = CL.EnqueueFillBuffer(
                ClCommandQueue,
                buffer.ClBuffer,
                arr,
                UIntPtr.Zero,
                (UIntPtr) (arr.Length * sizeof(T)),
                null,
                out clEvent);
     
            CLResultCode releaseResult = CL.ReleaseEvent(clEvent);
            result.ValidateSuccess();
            releaseResult.ValidateSuccess();
        }
    }

    public void EnqueueNdRangeKernel(Kernel kernel, Int32 size, IEnumerable<KernelArgValue> argValues) {
        CheckIfDisposed();
        kernel.CheckIfDisposed();

        Int32 i = 0;
        foreach (var value in argValues) {
            value.Set(kernel, i);
            ++i;
        }

        CLResultCode result;
        CLEvent clEvent;
        result = CL.EnqueueNDRangeKernel(
            ClCommandQueue,
            kernel.ClKernel,
            1,
            null,
            new UIntPtr[] {new UIntPtr((UInt32) size)},
            null,
            0,
            null,
            out clEvent);

        CLResultCode releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void EnqueueReadBuffer<T>(Buffer buffer, T[] arr) where T : unmanaged {
        CheckIfDisposed();
        buffer.CheckIfDisposed();

        CLResultCode result;
        CLEvent clEvent;
        result = CL.EnqueueReadBuffer(
            ClCommandQueue,
            buffer.ClBuffer,
            true,
            UIntPtr.Zero,
            arr,
            null,
            out clEvent);

        CLResultCode releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void Finish() {
        CheckIfDisposed();

        CL.Finish(ClCommandQueue).ValidateSuccess();
    }

    public bool IsDisposed { get; private set; } = false;

    private void ReleaseUnmanagedResources() {
        CL.ReleaseCommandQueue(ClCommandQueue);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || Context.IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~CommandQueue() {
        ReleaseUnmanagedResources();
    }
}
}