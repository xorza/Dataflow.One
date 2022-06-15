using System;
using System.Runtime.InteropServices;
using csso.Common;

namespace csso.ImageProcessing;

public class MemoryBuffer : IDisposable {
    public MemoryBuffer(UInt32 sizeInBytes) {
        SizeInBytes = sizeInBytes;
        Ptr = Memory.Alloc(sizeInBytes);
    }

    public unsafe MemoryBuffer(
        IntPtr ptr,
        UInt32 sizeInBytes,
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
        }
        else {
            Ptr = ptr;
        }
    }

    public UInt32 SizeInBytes { get; }
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

    public unsafe void Set<T>(UInt32 offsetInBytes, T value) where T : unmanaged {
        if (offsetInBytes + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(offsetInBytes));
        }

        var data = (T*) (Ptr + (Int32) offsetInBytes).ToPointer();
        *data = value;
    }


    public unsafe T Get<T>(UInt32 offsetInBytes) where T : unmanaged {
        if (offsetInBytes + sizeof(T) > SizeInBytes) {
            throw new ArgumentException(nameof(offsetInBytes));
        }

        var data = (T*) (Ptr + (Int32) offsetInBytes).ToPointer();
        return *data;
    }

    public byte[] GetBytes() {
        byte[] result = new byte[SizeInBytes];
        Marshal.Copy(Ptr, result, 0, (Int32) SizeInBytes);

        return result;
    }
}