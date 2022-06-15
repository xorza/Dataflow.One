using System;
using System.Runtime.InteropServices;
using csso.Common;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public unsafe class ClImage : IDisposable {
    internal CLImage RawClImage { get; }
    public bool IsDisposed { get; private set; }
    public ClContext ClContext { get; }

    public UInt32 Width { get; }
    public UInt32 Height { get; }
    public UInt32 Stride { get; }
    public UInt32 SizeInBytes { get; }
    public PixelFormat PixelFormat { get; }


    public ClImage(
        ClContext ctx,
        UInt32 width, UInt32 height,
        PixelFormat pixelFormat)
        : this(ctx, width, height, pixelFormat.CalculateStride(width), pixelFormat, IntPtr.Zero) { }

    public ClImage(
        ClContext ctx,
        UInt32 width, UInt32 height, UInt32 stride,
        PixelFormat pixelFormat)
        : this(ctx, width, height, stride, pixelFormat, IntPtr.Zero) { }

    public ClImage(ClContext ctx,
        UInt32 width, UInt32 height,
        PixelFormat pixelFormat,
        IntPtr data)
        : this(ctx, width, height, pixelFormat.CalculateStride(width), pixelFormat, data) { }

    public ClImage(ClContext ctx,
        UInt32 width, UInt32 height, UInt32 stride,
        PixelFormat pixelFormat,
        IntPtr data) {
        ClContext = ctx;

        Width = width;
        Height = height;
        Stride = stride;
        PixelFormat = pixelFormat;
        SizeInBytes = stride * height;

        var imageDescription = ImageDescription.Create2D(
            width, height, stride
        );
        var clImageFormat = ToImageFormat(pixelFormat);

        var memoryFlags = MemoryFlags.ReadWrite;
        if (data != IntPtr.Zero) {
            memoryFlags |= MemoryFlags.CopyHostPtr;
        }

        RawClImage = CL.CreateImage(
            ctx.RawClContext,
            memoryFlags,
            ref clImageFormat,
            ref imageDescription,
            data,
            out var result
        );
        result.ValidateSuccess();
    }

    private void ReleaseUnmanagedResources() {
        CL.ReleaseMemoryObject(RawClImage);
    }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ClImage() {
        ReleaseUnmanagedResources();
    }

    internal void CheckIfDisposed() {
        if (IsDisposed || ClContext.IsDisposed) throw new InvalidOperationException("Already disposed.");
    }

    public void Upload<T>(ClCommandQueue commandQueue, T[] data) where T : unmanaged {
        if (data.Length != Width * Height) {
            throw new Exception("qg4yo98hrvdf");
        }

        IntPtr dataPtr = Memory.Alloc(SizeInBytes);
        fixed (T* srcDataPtr = data) {
            for (UInt32 row = 0; row < Height; row++) {
                var srcDataRowPtr = srcDataPtr + row * Stride;
        
                Memory.Copy(
                    new IntPtr(srcDataRowPtr),
                    dataPtr + (Int32) (row * Stride),
                    Stride
                );
            }
        }

        Upload(commandQueue, dataPtr);
    }

    public void Upload(ClCommandQueue commandQueue, IntPtr data) {
        var result = CL.EnqueueWriteImage(
            commandQueue.RawClCommandQueue,
            RawClImage,
            true,
            new UIntPtr[3] {UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero},
            new UIntPtr[3] {new(Width), new(Height), new(1)},
            new UIntPtr(Stride),
            UIntPtr.Zero,
            data,
            0,
            null,
            out var clEvent
        );

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void Download<T>(ClCommandQueue commandQueue, T[] data) where T : unmanaged {
        var dataPtr = Memory.Alloc(SizeInBytes);
        Download(commandQueue,dataPtr);
        
        fixed (T* srcDataPtr = data) {
            for (UInt32 row = 0; row < Height; row++) {
                var srcDataRowPtr = srcDataPtr + row * Stride;
        
                Memory.Copy(
                    dataPtr + (Int32) (row * Stride),
                    new IntPtr(srcDataRowPtr),
                    Stride
                );
            }
        }
        
    }
    public void Download(ClCommandQueue commandQueue, IntPtr data) {
        var result = CL.EnqueueReadImage(
            commandQueue.RawClCommandQueue,
            RawClImage,
            true,
            new UIntPtr[3] {UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero},
            new UIntPtr[3] {new(Width), new(Height), new(1)},
            new UIntPtr(Stride),
            UIntPtr.Zero,
            data,
            0,
            null,
            out var clEvent
        );

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    private static ImageFormat ToImageFormat(PixelFormat pf) {
        switch (pf) {
            case PixelFormat.Rgb8:
                return new ImageFormat() {
                    ChannelOrder = ChannelOrder.Rgb,
                    ChannelType = ChannelType.UnsignedInteger8
                };
            case PixelFormat.Rgba8:
                return new ImageFormat() {
                    ChannelOrder = ChannelOrder.Rgba,
                    ChannelType = ChannelType.UnsignedInteger8
                };
            default:
                throw new Exception("q nc3y98 4849vg2785whg");
        }
    }
}