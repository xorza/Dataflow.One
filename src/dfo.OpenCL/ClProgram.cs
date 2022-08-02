using System;
using System.Collections.Generic;
using OpenTK.Compute.OpenCL;

namespace dfo.OpenCL;

public class ClProgram : IDisposable {
    public ClProgram(ClContext clContext, string code) {
        clContext.CheckIfDisposed();

        ClContext = clContext;
        Code = code;

        CLResultCode result;
        InternalClProgram = CL.CreateProgramWithSource(ClContext.RawClContext, code, out result);
        result.ValidateSuccess();

        result = CL.BuildProgram(
            InternalClProgram,
            (uint) ClContext.ClDevices.Length,
            ClContext.ClDevices,
            "-cl-kernel-arg-info",
            IntPtr.Zero,
            IntPtr.Zero);
        result.ValidateSuccess();

        List<ClKernel> kernels = new();
        Kernels = kernels.AsReadOnly();

        result = CL.GetProgramInfo(InternalClProgram, ProgramInfo.KernelNames, out var clKernelNames);
        result.ValidateSuccess();

        var kernelNames = clKernelNames.DecodeString().Split(';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var kernelName in kernelNames) {
            var clKernel = CL.CreateKernel(InternalClProgram, kernelName, out result);
            result.ValidateSuccess();
            kernels.Add(new ClKernel(this, kernelName, clKernel));
        }
    }

    public string Code { get; }
    public ClContext ClContext { get; }

    internal CLProgram InternalClProgram { get; }

    public IReadOnlyList<ClKernel> Kernels { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseProgram(InternalClProgram);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || ClContext.IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    ~ClProgram() {
        ReleaseUnmanagedResources();
    }
}