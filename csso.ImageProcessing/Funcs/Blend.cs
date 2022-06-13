using System;
using System.Linq;
using csso.Common;
using csso.NodeCore;
using csso.OpenCL;
using OpenTK.Mathematics;
using Vector3 = System.Numerics.Vector3;

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
        var width = a.Width;
        var height = a.Height;
        var stride = a.Stride;

        if (a.Width != b.Width
            || a.Height != b.Height
            || a.PixelFormatInfo != b.PixelFormatInfo) {
            throw new Exception("3cn9ty88g94");
        }

        var imagePool = _context.Get<ImagePool>();
        var resultImage = imagePool.Acquire(a.Width, a.Height, a.PixelFormatInfo.Pf);

        var aBuff = a.TakeGpuBuffer(Image.Operation.Read);
        var bBuff = b.TakeGpuBuffer(Image.Operation.Read);
        var resultBuff = resultImage.TakeGpuBuffer(Image.Operation.Write);
        var whs = new Vector4i(resultImage.Width, resultImage.Height, resultImage.Stride,0);

        KernelArgValue[] argsValues = {
            new BufferKernelArgValue(aBuff),
            new BufferKernelArgValue(bBuff),
            new BufferKernelArgValue(resultBuff),
            new ScalarKernelArgValue<Vector4i>(whs)
        };
        var workSize = new Int32[2] {resultImage.Width/3, resultImage.Height/3};


        var clContext = _context.Get<ClContext>();
        Init(clContext);
        var kernel = _clProgram!.Kernels.Single(_ => _.Name == "add");


        using (ClCommandQueue clCommandQueue = new(clContext)) {
            clCommandQueue.EnqueueNdRangeKernel(kernel, workSize, argsValues);
            clCommandQueue.Finish();
        }

        image = resultImage;

        return true;
    }


    private void Init(ClContext clContext) {
        if (_clProgram != null) return;

        var code = @"
                __kernel void add (
                    __global uchar4* A, 
                    __global uchar4* B,
                    __global uchar4* result,
                       const int3 whs
                )
                {
                    int x = get_global_id(0);
                    int y = get_global_id(1);
                    int i = y * whs.z + x;

                    result[i] = A[i] + B[i];
                }";

        _clProgram = new ClProgram(clContext, code);
    }
}