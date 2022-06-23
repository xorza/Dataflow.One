using System;
using System.Runtime.InteropServices;

namespace dfo.Common;

public static unsafe class Memory {
    public static IntPtr Alloc(UInt32 bytes) {
        return Marshal.AllocHGlobal((Int32) bytes);
    }

    public static void Copy(IntPtr src, IntPtr dst, UInt32 sizeInBytes) {
        Buffer.MemoryCopy(
            src.ToPointer(),
            dst.ToPointer(),
            sizeInBytes,
            sizeInBytes
        );
    }
}