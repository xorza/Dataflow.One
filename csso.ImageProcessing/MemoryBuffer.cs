using System;
using System.Runtime.InteropServices;

namespace csso.ImageProcessing;

public class MemoryBuffer : IDisposable {
    public Int32 SizeInBytes { get; }
    public IntPtr Ptr { get; }

    public unsafe MemoryBuffer(
        IntPtr ptr,
        Int32 sizeInBytes,
        bool copy) {
        SizeInBytes = sizeInBytes;

        if (copy) {
            Ptr = Marshal.AllocHGlobal(SizeInBytes);
            Buffer.MemoryCopy(
                (void*) ptr,
                (void*) Ptr,
                sizeInBytes,
                sizeInBytes
            );
        }
        else {
            Ptr = ptr;
        }
    }

    private void ReleaseUnmanagedResources() {
        if (Ptr != IntPtr.Zero) {
            Marshal.FreeHGlobal(Ptr);
        }
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~MemoryBuffer() {
        ReleaseUnmanagedResources();
    }

    // public unsafe T Get<T>(int x, int y) where T : unmanaged {
    //     var data = (T*) (_ptr + y * Stride).ToPointer();
    //     return data[x];
    // }
    // public T[] As<T>() where T : unmanaged {
    //     var result = new T[TotalPixels];
    //     for (var y = 0; y < Height; y++)
    //     for (var x = 0; x < Width; x++)
    //         result[y * Width + x] = Get<T>(x, y);
    //
    //     return result;
    // }
}