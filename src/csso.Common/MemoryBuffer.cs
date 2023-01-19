using System;
using System.Runtime.InteropServices;

namespace csso.Common;

public class MemoryBuffer : IDisposable {
    public MemoryBuffer(uint sizeInBytes) {
        SizeInBytes = sizeInBytes;
        Ptr = Memory.Alloc(sizeInBytes);
    }

    public unsafe MemoryBuffer(
        IntPtr ptr,
        uint sizeInBytes,
        bool copy) {
        SizeInBytes = sizeInBytes;

        if (copy) {
            Ptr = Memory.Alloc(sizeInBytes);
            Buffer.MemoryCopy(
                (void*) ptr,
                (void*) Ptr,
                sizeInBytes,
                sizeInBytes
            );
        } else {
            Ptr = ptr;
        }
    }

    public uint SizeInBytes { get; }
    public IntPtr Ptr { get; }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources() {
        if (Ptr != IntPtr.Zero) {
            Marshal.FreeHGlobal(Ptr);
        }
    }

    ~MemoryBuffer() {
        ReleaseUnmanagedResources();
    }

    public unsafe void Set<T>(uint offsetInBytes, T value) where T : unmanaged {
        if (offsetInBytes + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(offsetInBytes));
        }

        var data = (T*) (Ptr + (int) offsetInBytes).ToPointer();
        *data = value;
    }


    public unsafe T Get<T>(uint offsetInBytes) where T : unmanaged {
        if (offsetInBytes + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(offsetInBytes));
        }

        var data = (T*) (Ptr + (int) offsetInBytes).ToPointer();
        return *data;
    }

    public byte[] GetBytes() {
        var result = new byte[SizeInBytes];
        Marshal.Copy(Ptr, result, 0, (int) SizeInBytes);

        return result;
    }

    public void Upload(IntPtr data, uint offsetInBytes, uint sizeInBytes) {
        if (offsetInBytes + sizeInBytes > SizeInBytes) {
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
        }

        Memory.Copy(data, Ptr + (int) offsetInBytes, sizeInBytes);
    }
}