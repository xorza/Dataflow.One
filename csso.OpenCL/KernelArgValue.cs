using System;
using OpenTK.Compute.OpenCL;
using Buffer = csso.OpenCL.Buffer;

namespace csso.OpenCL {
public abstract class KernelArgValue {
    internal abstract void Set(Kernel kernel, Int32 index);
}

public class ScalarKernelArgValue<T> : KernelArgValue where T : unmanaged {
    public T Value { get; } = default(T);

    public ScalarKernelArgValue(T value) {
        Value = value;
    }

    internal override void Set(Kernel kernel, int index) {
        CL.SetKernelArg(kernel.ClKernel, (UInt32) index, Value).ValidateSuccess();
    }
}

public class BufferKernelArgValue : KernelArgValue {
    public Buffer Buffer { get; }

    public BufferKernelArgValue(Buffer buffer) {
        Buffer = buffer;
    }

    internal override void Set(Kernel kernel, int index) {
        CL.SetKernelArg(kernel.ClKernel, (UInt32) index, Buffer.ClBuffer).ValidateSuccess();
    }
}
}