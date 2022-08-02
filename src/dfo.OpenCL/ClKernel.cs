using System;
using System.Collections.Generic;
using System.Diagnostics;
using dfo.Common;
using OpenTK.Compute.OpenCL;

namespace dfo.OpenCL;

public class ClKernel : IDisposable {
    private readonly List<ClKernelArg> _args = new();

    internal ClKernel(ClProgram clProgram, string name, CLKernel clKernel) {
        clProgram.ClContext.CheckIfDisposed();

        Args = _args.AsReadOnly();

        InternalClKernel = clKernel;
        ClProgram = clProgram;
        Name = name;

        ValidateName();
        Inspect();
    }

    internal CLKernel InternalClKernel { get; }

    public ClProgram ClProgram { get; }
    public string Name { get; }
    public IReadOnlyList<ClKernelArg> Args { get; }


    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    [Conditional("DEBUG")]
    private void ValidateName() {
        CheckIfDisposed();

        CL.GetKernelInfo(InternalClKernel, KernelInfo.FunctionName, out var nameBytes)
            .ValidateSuccess();

        var name = nameBytes.DecodeString();
        Check.True(string.Equals(name, Name));
    }

    private void Inspect() {
        CheckIfDisposed();

        CLResultCode result;
        result = CL.GetKernelInfo(InternalClKernel, KernelInfo.NumberOfArguments, out var bytes);
        result.ValidateSuccess();

        var argCount = BitConverter.ToUInt32(bytes);
        for (uint i = 0; i < argCount; i++) {
            CL.GetKernelArgInfo(InternalClKernel, i, KernelArgInfo.Name, out bytes).ValidateSuccess();
            var argName = bytes.DecodeString();

            CL.GetKernelArgInfo(InternalClKernel, i, KernelArgInfo.TypeName, out bytes).ValidateSuccess();
            var typeName = bytes.DecodeString();

            _args.Add(new ClKernelArg(argName, typeName));

            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AccessQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AddressQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeQualifier, out bytes).ValidateSuccess();
        }
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseKernel(InternalClKernel);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || ClProgram.IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    ~ClKernel() {
        ReleaseUnmanagedResources();
    }
}