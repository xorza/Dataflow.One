using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class ClCommandQueue : IDisposable {
    public ClCommandQueue(ClContext clContext) {
        clContext.CheckIfDisposed();

        ClContext = clContext;

        RawClCommandQueue =
            CL.CreateCommandQueueWithProperties(
                clContext.RawClContext,
                clContext.SelectedClDevice,
                IntPtr.Zero,
                out var result);
        result.ValidateSuccess();
    }

    public ClContext ClContext { get; }

    internal CLCommandQueue RawClCommandQueue { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public void EnqueueWriteBuffer(ClBuffer clBuffer, IntPtr bytes) {
        CheckIfDisposed();
        clBuffer.CheckIfDisposed();

        CLResultCode result;
        CLEvent clEvent;
        result = CL.EnqueueWriteBuffer(
            RawClCommandQueue,
            clBuffer.RawClBuffer,
            true,
            UIntPtr.Zero,
            clBuffer.SizeInBytes,
            bytes,
            0,
            null,
            out clEvent);

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void EnqueueNdRangeKernel(ClKernel clKernel, int[] size, IEnumerable<ClKernelArgValue> argValues) {
        CheckIfDisposed();
        clKernel.CheckIfDisposed();

        var i = 0;
        foreach (var value in argValues) {
            value.Set(clKernel, i);
            ++i;
        }

        var globalWorkSize = size
            .Select(_ => (UIntPtr)_)
            .ToArray();

        CLResultCode result;
        CLEvent clEvent;
        result = CL.EnqueueNDRangeKernel(
            RawClCommandQueue,
            clKernel.InternalClKernel,
            (uint)globalWorkSize.Length,
            null,
            globalWorkSize,
            null,
            0,
            null,
            out clEvent);

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void EnqueueReadBuffer<T>(ClBuffer clBuffer, T[] arr) where T : unmanaged {
        CheckIfDisposed();
        clBuffer.CheckIfDisposed();

        CLResultCode result;
        CLEvent clEvent;
        result = CL.EnqueueReadBuffer(
            RawClCommandQueue,
            clBuffer.RawClBuffer,
            true,
            UIntPtr.Zero,
            arr,
            null,
            out clEvent);

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void EnqueueReadBuffer(ClBuffer clBuffer, IntPtr ptr) {
        CheckIfDisposed();
        clBuffer.CheckIfDisposed();

        CLResultCode result;
        CLEvent clEvent;
        result = CL.EnqueueReadBuffer(
            RawClCommandQueue,
            clBuffer.RawClBuffer,
            true,
            UIntPtr.Zero,
            clBuffer.SizeInBytes,
            ptr,
            0,
            null,
            out clEvent);

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void Finish() {
        CheckIfDisposed();

        CL.Finish(RawClCommandQueue).ValidateSuccess();
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseCommandQueue(RawClCommandQueue);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || ClContext.IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    ~ClCommandQueue() {
        ReleaseUnmanagedResources();
    }
}