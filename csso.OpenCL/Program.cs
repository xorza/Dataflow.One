﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
public class Program:IDisposable {
    public String Code { get; }
    public Context Context { get; }

    internal CLProgram ClProgram { get; }

    public IReadOnlyList<Kernel> Kernels { get; }

    public Program(Context context, String code) {
        context.CheckIfDisposed();

        Context = context;
        Code = code;

        CLResultCode result;
        ClProgram = CL.CreateProgramWithSource(Context.ClContext, code, out result);
        result.ValidateSuccess();

        result = CL.BuildProgram(
            ClProgram,
            (uint) Context.ClDevices.Length,
            Context.ClDevices,
            "-cl-kernel-arg-info",
            IntPtr.Zero,
            IntPtr.Zero);
        result.ValidateSuccess();

        List<Kernel> kernels = new List<Kernel>();
        Kernels = kernels.AsReadOnly();

        result = CL.GetProgramInfo(ClProgram, ProgramInfo.KernelNames, out byte[] clKernelNames);
        result.ValidateSuccess();

        String[] kernelNames = clKernelNames.DecodeString().Split(';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (string kernelName in kernelNames) {
            CLKernel clKernel = CL.CreateKernel(ClProgram, kernelName, out result);
            result.ValidateSuccess();
            kernels.Add(new Kernel(this, kernelName, clKernel));
        }
    }
    
    public bool IsDisposed { get; private set; } = false;

    private void ReleaseUnmanagedResources() {
        CL.ReleaseProgram(ClProgram);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || Context.IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Program() {
        ReleaseUnmanagedResources();
    }
}
}