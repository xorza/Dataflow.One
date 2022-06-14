using System;
using System.Linq;
using csso.Common;
using csso.NodeCore;
using csso.OpenCL;
using OpenTK.Mathematics;

namespace csso.ImageProcessing.Funcs;

public class Blend : Function, IDisposable {
    private readonly Context _context;

    private ClProgram? _clProgram;

    public Blend(Context ctx) {
        _context = ctx;
        Name = "Blend";

        SetFunction(Do);
    }

    public void Dispose() { }

    public bool Do(Image a, Image b, [Output] out Image image) {
        var clContext = _context.Get<ClContext>();
        Init(clContext);

        var width = a.Width;
        var height = a.Height;
        var sizeInBytes = a.SizeInBytes;
        var pixelFormatInfo = a.PixelFormatInfo;

        if (width != b.Width
            || height != b.Height
            || sizeInBytes != b.SizeInBytes
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
        KernelArgValue[] argsValues = {
            new BufferKernelArgValue(aBuff),
            new BufferKernelArgValue(bBuff),
            new BufferKernelArgValue(resultBuff)
        };
        var workSize = new Int32[] {width, height};

        using (ClCommandQueue clCommandQueue = new(clContext)) {
            clCommandQueue.EnqueueNdRangeKernel(kernel, workSize, argsValues);
            clCommandQueue.Finish();
        }

        image = resultImage;

        return true;
    }


    private void Init(ClContext clContext) {
        if (_clProgram != null) return;

        const String code = @"
            kernel
            void add(global const uchar4* A, 
                      global const uchar4* B,
                      global uchar4* result) {
                int x = get_global_id(0);
                int y = get_global_id(1);    
                int i = y * get_global_size(0) + x;

                float4 fa = convert_float4(A[i]) / (float4)(255.0);
                float4 fb = convert_float4(B[i]) / (float4)(255.0);
                float4 fresult = (fa * fb) * (float4)(255.0);

                result[i] = convert_uchar4(fresult);
            }
            ";

        _clProgram = new ClProgram(clContext, code);
    }
}