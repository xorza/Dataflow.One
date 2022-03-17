using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public abstract class KernelArgValue {
    internal abstract void Set(Kernel kernel, int index);
}

public class ScalarKernelArgValue<T> : KernelArgValue where T : unmanaged {
    public ScalarKernelArgValue(T value) {
        Value = value;
    }

    public T Value { get; }

    internal override void Set(Kernel kernel, int index) {
        CL.SetKernelArg(kernel.ClKernel, (uint) index, Value).ValidateSuccess();
    }
}

public class BufferKernelArgValue : KernelArgValue {
    public BufferKernelArgValue(Buffer buffer) {
        Buffer = buffer;
    }

    public Buffer Buffer { get; }

    internal override void Set(Kernel kernel, int index) {
        CL.SetKernelArg(kernel.ClKernel, (uint) index, Buffer.ClBuffer).ValidateSuccess();
    }
}