using System;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
public class Buffer : IDisposable {
    public Context Context { get; }
    public Int32 SizeInBytes { get; private set; } = 0;

    internal CLBuffer ClBuffer { get; }

    public Buffer(Context context, Int32 sizeInBytes) {
        context.CheckIfDisposed();

        Context = context;

        CLResultCode result;
        ClBuffer = CL.CreateBuffer(
            Context.ClContext,
            MemoryFlags.WriteOnly,
            new UIntPtr((UInt32) sizeInBytes),
            IntPtr.Zero,
            out result);
        result.ValidateSuccess();

        SizeInBytes = sizeInBytes;
    }

    private Buffer(Context context, CLBuffer clBuffer, Int32 sizeInBytes) {
        context.CheckIfDisposed();
        Check.Argument(sizeInBytes > 0, nameof(sizeInBytes));

        Context = context;
        ClBuffer = clBuffer;
        SizeInBytes = sizeInBytes;
    }

    public static Buffer Create<T>(Context context, T[] arr) where T : unmanaged {
        context.CheckIfDisposed();
        Check.Argument(arr.Length > 0, nameof(arr));

        CLResultCode result;
        CLBuffer clBuffer = CL.CreateBuffer(
            context.ClContext,
            MemoryFlags.ReadOnly | MemoryFlags.CopyHostPtr,
            arr,
            out result);
        result.ValidateSuccess();

        unsafe {
            return new Buffer(context, clBuffer, arr.Length * sizeof(T));
        }
    }

    public bool IsDisposed { get; private set; } = false;

    private void ReleaseUnmanagedResources() {
        CL.ReleaseMemoryObject(ClBuffer);
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

    ~Buffer() {
        ReleaseUnmanagedResources();
    }
}
}