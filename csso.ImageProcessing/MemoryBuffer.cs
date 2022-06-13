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

    public unsafe T Get<T>(int i) where T : unmanaged {
        int offset = i * sizeof(T);
        if (offset < 0 || offset + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(i));
        }

        var data = (T*) (Ptr + offset).ToPointer();
        return *data;
    }

    public unsafe void Set<T>(int i, T value) where T : unmanaged {
        int offset = i * sizeof(T);
        if (offset < 0 || offset + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(i));
        }

        var data = (T*) (Ptr + offset).ToPointer();
        *data = value;
    }


    public unsafe T[] As<T>() where T : unmanaged {
        var result = new T[SizeInBytes / sizeof(T)];
        for (var i = 0; i < result.Length; i++)
            result[i] = Get<T>(i);
        return result;
    }
}