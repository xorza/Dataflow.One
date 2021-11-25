using System;
using OpenTK.Compute.OpenCL;

namespace csso.ImageProcessing {
public class Program {
    public String Code { get; }
    public Context Context { get; }

    internal CLProgram ClProgram { get;  }
    internal CLKernel[] ClKernels { get;  }

    public Program(Context context, String code) {
        context.CheckIfDisposed();

        Context = context;
        Code = code;

        CLResultCode result;
        ClProgram = CL.CreateProgramWithSource(Context.ClContext, code, out result);

        result = CL.BuildProgram(ClProgram, (uint) Context.ClDevices.Length, Context.ClDevices, null, IntPtr.Zero,
            IntPtr.Zero);


        result =    CL.CreateKernelsInProgram(ClProgram, out CLKernel[] kernels);
        ClKernels = kernels;
        
    }
}
}