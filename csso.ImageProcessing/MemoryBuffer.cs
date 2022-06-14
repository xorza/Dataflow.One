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

    public unsafe void Set<T>(int offsetInBytes, T value) where T : unmanaged {
        if (offsetInBytes < 0 || offsetInBytes + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(offsetInBytes));
        }

        var data = (T*) (Ptr + offsetInBytes).ToPointer();
        *data = value;
    }


    public unsafe T Get<T>(int offsetInBytes) where T : unmanaged {
        if (offsetInBytes < 0 || offsetInBytes + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(offsetInBytes));
        }

        var data = (T*) (Ptr + offsetInBytes).ToPointer();
        return *data;
    }

    public byte[] GetBytes() {
        byte[] result = new byte[SizeInBytes];
        Marshal.Copy(Ptr, result, 0, SizeInBytes);

        return result;
    }
}