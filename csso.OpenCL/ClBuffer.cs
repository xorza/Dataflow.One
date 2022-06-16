using System;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class ClBuffer : IDisposable {
    public ClBuffer(ClContext clContext, UInt32 sizeInBytes) {
        clContext.CheckIfDisposed();

        ClContext = clContext;

        CLResultCode result;
        RawClBuffer = CL.CreateBuffer(
            ClContext.RawClContext,
            MemoryFlags.ReadWrite,
            new UIntPtr((uint) sizeInBytes),
            IntPtr.Zero,
            out result);
        result.ValidateSuccess();

        SizeInBytes = sizeInBytes;
    }

    private ClBuffer(
        ClContext clContext,
        CLBuffer clRawClBuffer,
        UInt32 sizeInBytes) {
        clContext.CheckIfDisposed();
        Check.Argument(sizeInBytes > 0, nameof(sizeInBytes));

        ClContext = clContext;
        RawClBuffer = clRawClBuffer;
        SizeInBytes = sizeInBytes;
    }

    public ClContext ClContext { get; }
    public UInt32 SizeInBytes { get; }

    internal CLBuffer RawClBuffer { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public static ClBuffer Create<T>(ClContext clContext, T[] arr) where T : unmanaged {
        clContext.CheckIfDisposed();
        Check.Argument(arr.Length > 0, nameof(arr));

        CLResultCode result;
        var clBuffer = CL.CreateBuffer(
            clContext.RawClContext,
            MemoryFlags.ReadWrite | MemoryFlags.CopyHostPtr,
            arr,
            out result);
        result.ValidateSuccess();

        unsafe {
            return new ClBuffer(
                clContext,
                clBuffer,
                (UInt32) (arr.Length * sizeof(T))
            );
        }
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseMemoryObject(RawClBuffer);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || ClContext.IsDisposed) throw new InvalidOperationException("Already disposed.");
    }

    ~ClBuffer() {
        ReleaseUnmanagedResources();
    }
}