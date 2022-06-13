using System;
using System.Linq;
using csso.NodeCore;
using csso.OpenCL;

namespace csso.ImageProcessing.Funcs;

public class Blend : Function, IDisposable {
    private readonly Context _context;

    private ClProgram? _clProgram;

    public Blend(Context ctx) {
        _context = ctx;
        Name = "Blend";

        SetFunction(Blend_Func);
    }

    public void Dispose() { }

    private bool Blend_Func(Image a, Image b, [Output] out Image? image) {
        var clContext = _context.Get<ClContext>();

        Init(clContext);
        
        var imagePool = _context.Get<ImagePool>();
        var resultImage = imagePool.Acquire(a.Width, a.Height, a.PixelFormatInfo.Pf);

        ClCommandQueue clCommandQueue = new(clContext);

        KernelArgValue[] argsValues = {
            new BufferKernelArgValue(a.GpuBuffer!),
            new BufferKernelArgValue(b.GpuBuffer!),
            new BufferKernelArgValue(resultImage.GpuBuffer!)
        };

        var kernel = _clProgram!.Kernels.Single(_ => _.Name == "add");

        try {
            clCommandQueue.EnqueueNdRangeKernel(kernel, a.TotalPixels, argsValues);
            clCommandQueue.Finish();
        }
        finally {
            clCommandQueue.Dispose();
        }

        image = resultImage;

        return true;
    }


    private void Init(ClContext clContext) {
        if (_clProgram != null) {
            return;
        }

        var code = @"
                __kernel void add(__global float* A, __global float* B,__global float* result, const float C)
                {
                    int i = get_global_id(0);
                    result[i] = (A[i] + B[i]);
                }";

        _clProgram = new(clContext, code);
    }
}