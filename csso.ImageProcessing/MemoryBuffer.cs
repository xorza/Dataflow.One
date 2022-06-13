using System;
using System.Runtime.InteropServices;

namespace csso.ImageProcessing;

public class MemoryBuffer : IDisposable {
    public MemoryBuffer(int sizeInBytes) {
        SizeInBytes = sizeInBytes;
        Ptr = Marshal.AllocHGlobal(sizeInBytes);
    }

    public unsafe MemoryBuffer(
        IntPtr ptr,
        int sizeInBytes,
        bool copy) {
        SizeInBytes = sizeInBytes;

        if (copy) {
            Ptr = Marshal.AllocHGlobal(sizeInBytes);
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

    public int SizeInBytes { get; }
    public IntPtr Ptr { get; }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources() {
        if (Ptr != IntPtr.Zero) Marshal.FreeHGlobal(Ptr);
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