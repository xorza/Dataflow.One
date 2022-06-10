using System;
using System.Collections.Generic;
using System.Diagnostics;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class Kernel : IDisposable {
    private readonly List<KernelArg> _args = new();

    internal Kernel(Program program, string name, CLKernel clKernel) {
        program.ClContext.CheckIfDisposed();

        Args = _args.AsReadOnly();

        ClKernel = clKernel;
        Program = program;
        Name = name;

        ValidateName();
        Inspect();
    }

    internal CLKernel ClKernel { get; }

    public Program Program { get; }
    public string Name { get; }
    public IReadOnlyList<KernelArg> Args { get; }


    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    [Conditional("DEBUG")]
    private void ValidateName() {
        CheckIfDisposed();

        CL.GetKernelInfo(ClKernel, KernelInfo.FunctionName, out var nameBytes)
            .ValidateSuccess();

        var name = nameBytes.DecodeString();
        Check.True(string.Equals(name, Name));
    }

    private void Inspect() {
        CheckIfDisposed();

        CLResultCode result;
        result = CL.GetKernelInfo(ClKernel, KernelInfo.NumberOfArguments, out var bytes);
        result.ValidateSuccess();

        var argCount = BitConverter.ToUInt32(bytes);
        for (uint i = 0; i < argCount; i++) {
            CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.Name, out bytes).ValidateSuccess();
            var argName = bytes.DecodeString();

            CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeName, out bytes).ValidateSuccess();
            var typeName = bytes.DecodeString();

            _args.Add(new KernelArg(argName, typeName));

            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AccessQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AddressQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeQualifier, out bytes).ValidateSuccess();
        }
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseKernel(ClKernel);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || Program.IsDisposed) throw new InvalidOperationException("Already disposed.");
    }

    ~Kernel() {
        ReleaseUnmanagedResources();
    }
}