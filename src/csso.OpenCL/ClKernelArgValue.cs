using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public abstract class ClKernelArgValue {
    internal abstract void Set(ClKernel clKernel, int index);
}

public class ScalarClKernelArgValue<T> : ClKernelArgValue where T : unmanaged {
    public ScalarClKernelArgValue(T value) {
        Value = value;
    }

    public T Value { get; }

    internal override void Set(ClKernel clKernel, int index) {
        CL.SetKernelArg(clKernel.InternalClKernel, (uint)index, Value).ValidateSuccess();
    }
}

public class BufferClKernelArgValue : ClKernelArgValue {
    public BufferClKernelArgValue(ClBuffer clBuffer) {
        ClBuffer = clBuffer;
    }

    public ClBuffer ClBuffer { get; }

    internal override void Set(ClKernel clKernel, int index) {
        CL.SetKernelArg(clKernel.InternalClKernel, (uint)index, ClBuffer.RawClBuffer).ValidateSuccess();
    }
}

public class ImageClKernelArgValue : ClKernelArgValue {
    public ImageClKernelArgValue(ClImage clImage) {
        ClImage = clImage;
    }

    public ClImage ClImage { get; }

    internal override void Set(ClKernel clKernel, int index) {
        CL.SetKernelArg(clKernel.InternalClKernel, (uint)index, ClImage.RawClImage).ValidateSuccess();
    }
}