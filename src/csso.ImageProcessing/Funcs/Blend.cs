using System;
using System.Linq;
using csso.Common;
using csso.NodeCore;
using csso.OpenCL;

namespace csso.ImageProcessing.Funcs;

public class Blend : Function, IDisposable {
    private readonly Context _context;

    private ClProgram? _clProgram;

    public Blend(Context ctx) {
        _context = ctx;

        SetFunction(Do);

        Name = "Blend";
        Behavior = FunctionBehavior.Reactive;
    }

    public void Dispose() {
    }

    [Reactive]
    public bool Do(Image a, Image b, [Output] out Image image) {
        var clContext = _context.Get<ClContext>();
        Init(clContext);

        var width = a.Width;
        var height = a.Height;
        var sizeInBytes = a.SizeInBytes;
        var pixelFormatInfo = a.PixelFormatInfo;

        if (width != b.Width
            || height != b.Height
            || pixelFormatInfo != b.PixelFormatInfo) {
            throw new Exception("3cn9ty88g94");
        }

        var imagePool = _context.Get<ImagePool>();
        var resultImage = imagePool.Acquire(width, height);

        Debug.Assert.True(resultImage.SizeInBytes == sizeInBytes);

        var aBuff = a.TakeGpuBuffer(Image.Operation.Read);
        var bBuff = b.TakeGpuBuffer(Image.Operation.Read);
        var resultBuff = resultImage.TakeGpuBuffer(Image.Operation.Write);

        var kernel = _clProgram!.Kernels.Single(_ => _.Name == "add");
        ClKernelArgValue[] argsValues = {
            new ImageClKernelArgValue(aBuff),
            new ImageClKernelArgValue(bBuff),
            new ImageClKernelArgValue(resultBuff)
        };
        var workSize = new int[2] { (int)width, (int)height };

        using (ClCommandQueue clCommandQueue = new(clContext)) {
            clCommandQueue.EnqueueNdRangeKernel(kernel, workSize, argsValues);
            clCommandQueue.Finish();
        }

        image = resultImage;

        return true;
    }


    private void Init(ClContext clContext) {
        if (_clProgram != null) {
            return;
        }

        const string code = @"
            __constant sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE
                                           | CLK_ADDRESS_CLAMP_TO_EDGE   
                                           | CLK_FILTER_NEAREST;

            kernel
            void add( read_only  image2d_t A, 
                      read_only  image2d_t B,
                      write_only image2d_t result
                    ) {
                int2 coord = (int2)(get_global_id(0), get_global_id(1));
                
                float4 colorA = read_imagef(A, sampler, coord);
                float4 colorB = read_imagef(B, sampler, coord);

                write_imagef(result, coord, colorA * colorB);
            }
            ";

        _clProgram = new ClProgram(clContext, code);
    }
}