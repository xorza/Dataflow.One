using System;
using System.Runtime.InteropServices;

namespace csso.Common;

public static unsafe class Memory {
    public static IntPtr Alloc(uint bytes) {
        return Marshal.AllocHGlobal((int) bytes);
    }

    public static void Copy(IntPtr src, IntPtr dst, uint sizeInBytes) {
        Buffer.MemoryCopy(
            src.ToPointer(),
            dst.ToPointer(),
            sizeInBytes,
            sizeInBytes
        );
    }
}