using System;
using System.Collections.Generic;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
public class CommandQueue : IDisposable {
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

    public Context Context { get; }

    internal CLCommandQueue ClCommandQueue { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
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

            var releaseResult = CL.ReleaseEvent(clEvent);
            result.ValidateSuccess();
            releaseResult.ValidateSuccess();
        }
    }

    public void EnqueueNdRangeKernel(Kernel kernel, int size, IEnumerable<KernelArgValue> argValues) {
        CheckIfDisposed();
        kernel.CheckIfDisposed();

        var i = 0;
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
            new UIntPtr[] {new UIntPtr((uint) size)},
            null,
            0,
            null,
            out clEvent);

        var releaseResult = CL.ReleaseEvent(clEvent);
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

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void Finish() {
        CheckIfDisposed();

        CL.Finish(ClCommandQueue).ValidateSuccess();
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseCommandQueue(ClCommandQueue);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || Context.IsDisposed) throw new InvalidOperationException("Already disposed.");
    }

    ~CommandQueue() {
        ReleaseUnmanagedResources();
    }
}
}