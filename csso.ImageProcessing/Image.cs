using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using csso.Common;
using csso.OpenCL;

namespace csso.ImageProcessing;

public struct PixelFormatInfo {
    public PixelFormat PixelFormat { get; } = PixelFormat.Format32bppArgb;
    public int ChannelCount { get; } = 4;
    public int BytesPerChannel { get; } = 1;

    public PixelFormatInfo(PixelFormat pixelFormat) {
        PixelFormat = pixelFormat;

        switch (pixelFormat) {
            case PixelFormat.Format24bppRgb:
                ChannelCount = 3;
                BytesPerChannel = 1;
                break;

            default:
                throw new Exception(nameof(PixelFormat));
        }
    }
}

public class Image : IDisposable {
    private MemoryBuffer _cpuBuffer;
    private ClBuffer? _gpuBuffer;

    private readonly Context _context;

    public int Height { get; }
    public int Width { get; }
    public int Stride { get; }
    public int SizeInBytes { get; }
    public int TotalPixels => Height * Width;
    public PixelFormatInfo PixelFormatInfo { get; }
    
    public Image(Context ctx, FileInfo fileInfo) {
        _context = ctx;
        
        Bitmap img;
        using (var fileStream = fileInfo.OpenRead()) {
            img = new Bitmap(fileStream);
        }

        BitmapData? imageData = null;

        try {
            imageData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly,
                img.PixelFormat);

            PixelFormatInfo = new PixelFormatInfo(imageData.PixelFormat);
            SizeInBytes = imageData.Height * imageData.Stride;
            Height = imageData.Height;
            Width = imageData.Width;
            Stride = imageData.Stride;

            unsafe {
                _cpuBuffer = new MemoryBuffer((IntPtr) imageData.Scan0.ToPointer(), SizeInBytes, true);
            }
        }
        finally {
            if (imageData != null) {
                img.UnlockBits(imageData);
            }

            img.Dispose();
        }

        MoveToGpu(_context.Resolve<ClContext>());
    }

    public void Dispose() {
        _gpuBuffer?.Dispose();
        _cpuBuffer?.Dispose();
    }

    private void MoveToGpu(ClContext context) {
        if (_gpuBuffer == null
            || _gpuBuffer.SizeInBytes != _cpuBuffer.SizeInBytes) {
            _gpuBuffer?.Dispose();
            _gpuBuffer = new ClBuffer(context, _cpuBuffer.SizeInBytes);
        }

        var commandQueue = new CommandQueue(context);
        commandQueue.EnqueueWriteBuffer(_gpuBuffer, _cpuBuffer.Ptr);
        commandQueue.Finish();
    }
}