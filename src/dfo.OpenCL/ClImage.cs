using System;
using System.Runtime.InteropServices;
using dfo.Common;
using OpenTK.Compute.OpenCL;

namespace dfo.OpenCL;

public unsafe class ClImage : IDisposable {
    internal CLImage RawClImage { get; }
    public bool IsDisposed { get; private set; }
    public ClContext ClContext { get; }

    public uint Width { get; }
    public uint Height { get; }
    public uint Stride { get; }
    public uint SizeInBytes { get; }
    public PixelFormat PixelFormat { get; }

    public ClImage(
        ClContext ctx,
        uint width, uint height,
        PixelFormat pixelFormat,
        MemoryBuffer? buffer = null)
        : this(ctx, width, height, pixelFormat.CalculateStride(width), pixelFormat, buffer) { }

    public ClImage(ClContext ctx,
        uint width, uint height, uint stride,
        PixelFormat pixelFormat,
        MemoryBuffer? buffer = null) {
        ClContext = ctx;

        Width = width;
        Height = height;
        Stride = stride;
        PixelFormat = pixelFormat;
        SizeInBytes = stride * height;

        var memoryFlags = MemoryFlags.ReadWrite;
        if (buffer != null) {
            if (SizeInBytes != buffer.SizeInBytes) throw new Exception("ewgurualvbd4");

            memoryFlags |= MemoryFlags.CopyHostPtr;
        }

        var imageDescription = ImageDescription.Create2D(
            width, height, stride
        );
        var clImageFormat = ToImageFormat(pixelFormat);

        RawClImage = CL.CreateImage(
            ctx.RawClContext,
            memoryFlags,
            ref clImageFormat,
            ref imageDescription,
            buffer?.Ptr ?? IntPtr.Zero,
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
        if (data.Length != Width * Height) throw new Exception("qg4yo98hrvdf");

        using var buffer = new MemoryBuffer(SizeInBytes);
        fixed (T* srcDataPtr = data) {
            for (uint row = 0; row < Height; row++) {
                var srcDataRowPtr = srcDataPtr + row * Stride;

                Memory.Copy(
                    new IntPtr(srcDataRowPtr),
                    buffer.Ptr + (int) (row * Stride),
                    Stride
                );
            }
        }

        Upload(commandQueue, buffer);
    }

    public void Upload(ClCommandQueue commandQueue, MemoryBuffer buffer) {
        var result = CL.EnqueueWriteImage(
            commandQueue.RawClCommandQueue,
            RawClImage,
            true,
            new UIntPtr[3] {UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero},
            new UIntPtr[3] {new(Width), new(Height), new(1)},
            new UIntPtr(Stride),
            UIntPtr.Zero,
            buffer.Ptr,
            0,
            null,
            out var clEvent
        );

        var releaseResult = CL.ReleaseEvent(clEvent);
        result.ValidateSuccess();
        releaseResult.ValidateSuccess();
    }

    public void Download<T>(ClCommandQueue commandQueue, T[] data) where T : unmanaged {
        var buffer = new MemoryBuffer(SizeInBytes);
        Download(commandQueue, buffer);

        fixed (T* srcDataPtr = data) {
            for (uint row = 0; row < Height; row++) {
                var dstDataRowPtr = (byte*) srcDataPtr + row * Stride;

                Memory.Copy(
                    buffer.Ptr + (int) (row * Stride),
                    new IntPtr(dstDataRowPtr),
                    (uint) (Width * sizeof(T))
                );
            }
        }
    }

    public void Download(ClCommandQueue commandQueue, MemoryBuffer buffer) {
        var result = CL.EnqueueReadImage(
            commandQueue.RawClCommandQueue,
            RawClImage,
            true,
            new UIntPtr[3] {UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero},
            new UIntPtr[3] {new(Width), new(Height), new(1)},
            new UIntPtr(Stride),
            UIntPtr.Zero,
            buffer.Ptr,
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
            case PixelFormat.Rgba8:
                return new ImageFormat() {
                    ChannelOrder = ChannelOrder.Rgba,
                    ChannelType = ChannelType.NormalizedUnsignedInteger8
                };
            default:
                throw new Exception("q nc3y98 4849vg2785whg");
        }
    }
}