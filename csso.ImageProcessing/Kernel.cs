using System;
using System.Collections.Generic;
using System.Diagnostics;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.ImageProcessing {

public class Kernel {
    internal CLKernel ClKernel { get; }

    public Program Program { get; }
    public String Name { get; }

    public IReadOnlyList<KernelArg> Args { get; }

    internal Kernel(Program program, String name, CLKernel clKernel) {
        program.Context.CheckIfDisposed();

        ClKernel = clKernel;
        Program = program;
        Name = name;

        ValidateName();
        Inspect();
    }

    [Conditional("DEBUG")]
    private void ValidateName() {
        Program.Context.CheckIfDisposed();

        CLResultCode result;
        CL.GetKernelInfo(ClKernel, KernelInfo.FunctionName, out byte[] nameBytes)
            .ValidateSuccess();

        String name = nameBytes.DecodeString();
        Check.True(String.Equals(name, Name));
    }

    private void Inspect() {
        Program.Context.CheckIfDisposed();

        CLResultCode result;
        result = CL.GetKernelInfo(ClKernel, KernelInfo.NumberOfArguments, out byte[] bytes);
        result.ValidateSuccess();

        List<KernelArg> argsList = new();
        UInt32 argCount = BitConverter.ToUInt32(bytes);
        for (UInt32 i = 0; i < argCount; i++) {
            CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.Name, out bytes).ValidateSuccess();
            String argName = bytes.DecodeString();

            CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeName, out bytes).ValidateSuccess();
            String typeName = bytes.DecodeString();

            argsList.Add(new KernelArg(argName, typeName));

            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AccessQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.AddressQualifier, out bytes).ValidateSuccess();
            // CL.GetKernelArgInfo(ClKernel, i, KernelArgInfo.TypeQualifier, out bytes).ValidateSuccess();
        }
    }
}
}