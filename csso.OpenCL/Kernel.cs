using System;
using System.Collections.Generic;
using System.Diagnostics;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {

public class Kernel:IDisposable {
    internal CLKernel ClKernel { get; }

    public Program Program { get; }
    public String Name { get; }

    
    List<KernelArg> _args = new();
    public IReadOnlyList<KernelArg> Args { get; }

    internal Kernel(Program program, String name, CLKernel clKernel) {
        program.Context.CheckIfDisposed();
        
        Args = _args.AsReadOnly();

        ClKernel = clKernel;
        Program = program;
        Name = name;

        ValidateName();
        Inspect();
    }

    [Conditional("DEBUG")]
    private void ValidateName() {
        CheckIfDisposed();

        CL.GetKernelInfo(ClKernel, KernelInfo.FunctionName, out byte[] nameBytes)
            .ValidateSuccess();

        String name = nameBytes.DecodeString();
        Check.True(String.Equals(name, Name));
    }

    private void Inspect() {
        CheckIfDisposed();

        CLResultCode result;
        result = CL.GetKernelInfo(ClKernel, KernelInfo.NumberOfArguments, out byte[] bytes);
        result.ValidateSuccess();
        
        UInt32 argCount = BitConverter.ToUInt32(bytes);
        for (UInt32 i = 0; i < argCount; i++) {
            CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.Name, out bytes).ValidateSuccess();
            String argName = bytes.DecodeString();

            CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeName, out bytes).ValidateSuccess();
            String typeName = bytes.DecodeString();

            _args.Add(new KernelArg(argName, typeName));

            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AccessQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AddressQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeQualifier, out bytes).ValidateSuccess();
        }
    }


    public bool IsDisposed { get; private set; } = false;

    private void ReleaseUnmanagedResources() {
        CL.ReleaseKernel(ClKernel);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || Program.IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Kernel() {
        ReleaseUnmanagedResources();
    }
}
}