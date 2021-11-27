using System;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
public class Buffer : IDisposable {
    public Buffer(Context context, int sizeInBytes) {
        context.CheckIfDisposed();

        Context = context;

        CLResultCode result;
        ClBuffer = CL.CreateBuffer(
            Context.ClContext,
            MemoryFlags.WriteOnly,
            new UIntPtr((uint) sizeInBytes),
            IntPtr.Zero,
            out result);
        result.ValidateSuccess();

        SizeInBytes = sizeInBytes;
    }

    private Buffer(Context context, CLBuffer clBuffer, int sizeInBytes) {
        context.CheckIfDisposed();
        Check.Argument(sizeInBytes > 0, nameof(sizeInBytes));

        Context = context;
        ClBuffer = clBuffer;
        SizeInBytes = sizeInBytes;
    }

    public Context Context { get; }
    public int SizeInBytes { get; }

    internal CLBuffer ClBuffer { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public static Buffer Create<T>(Context context, T[] arr) where T : unmanaged {
        context.CheckIfDisposed();
        Check.Argument(arr.Length > 0, nameof(arr));

        CLResultCode result;
        var clBuffer = CL.CreateBuffer(
            context.ClContext,
            MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr,
            arr,
            out result);
        result.ValidateSuccess();

        unsafe {
            return new Buffer(context, clBuffer, arr.Length * sizeof(T));
        }
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseMemoryObject(ClBuffer);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || Context.IsDisposed) throw new InvalidOperationException("Already disposed.");
    }

    ~Buffer() {
        ReleaseUnmanagedResources();
    }
}
}