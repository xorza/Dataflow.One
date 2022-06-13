using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using csso.OpenCL;
using DrawingImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using WindowsMediaPixelFormat = System.Windows.Media.PixelFormat;

namespace csso.ImageProcessing;

public enum PixelFormat {
    Rgb8,
    Rgba8
}

public struct PixelFormatInfo {
    public static readonly PixelFormatInfo[] All = {
        new() {
            Dipf = DrawingImagingPixelFormat.Format32bppArgb,
            Wmpf = System.Windows.Media.PixelFormats.Bgra32,
            Pf = PixelFormat.Rgba8,
            ChannelCount = 4,
            BytesPerChannel = 1
        },
        new() {
            Dipf = DrawingImagingPixelFormat.Format24bppRgb,
            Wmpf = System.Windows.Media.PixelFormats.Bgr24,
            Pf = PixelFormat.Rgb8,
            ChannelCount = 3,
            BytesPerChannel = 1
        }
    };

    public static PixelFormatInfo Get(DrawingImagingPixelFormat pf) {
        return All.Single(_ => _.Dipf == pf);
    }

    public static PixelFormatInfo Get(WindowsMediaPixelFormat pf) {
        return All.Single(_ => _.Wmpf == pf);
    }

    public static PixelFormatInfo Get(PixelFormat pf) {
        return All.Single(_ => _.Pf == pf);
    }

    public PixelFormat Pf { get; private set; }
    public DrawingImagingPixelFormat Dipf { get; private set; }
    public WindowsMediaPixelFormat Wmpf { get; private set; }
    public int ChannelCount { get; private set; }
    public int BytesPerChannel { get; private set; }
}

public class Image : IDisposable {
    internal MemoryBuffer? CpuBuffer { get; private set; }
    internal ClBuffer? GpuBuffer { get; private set; }

    private readonly Context _context;

    private const Int32 StrideAlignment = 64;

    public int Height { get; }
    public int Width { get; }
    public Int32 TotalPixels {
        get => Height * Width;
    }

    public int Stride { get; }
    public int SizeInBytes { get; }
    public PixelFormatInfo PixelFormatInfo { get; }

    public Image(Context ctx,
        Int32 width, Int32 height, PixelFormat pixelFormat) {
        _context = ctx;
        Width = width;
        Height = height;
        var pixelFormatInfo = PixelFormatInfo.Get(pixelFormat);
        PixelFormatInfo = pixelFormatInfo;
        var bytesPerRow = width * pixelFormatInfo.BytesPerChannel * pixelFormatInfo.ChannelCount;
        Stride = (bytesPerRow / StrideAlignment) * StrideAlignment;

        Stride = (bytesPerRow + (StrideAlignment - 1)) & ~(StrideAlignment - 1);
    }

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

            PixelFormatInfo = PixelFormatInfo.Get(imageData.PixelFormat);
            SizeInBytes = imageData.Height * imageData.Stride;
            Height = imageData.Height;
            Width = imageData.Width;
            Stride = imageData.Stride;

            unsafe {
                CpuBuffer = new MemoryBuffer((IntPtr) imageData.Scan0.ToPointer(), SizeInBytes, true);
            }
        }
        finally {
            if (imageData != null) {
                img.UnlockBits(imageData);
            }

            img.Dispose();
        }

        CopyToGpu();
    }

    public void Dispose() {
        GpuBuffer?.Dispose();
        CpuBuffer?.Dispose();
    }

    private void CopyToGpu() {
        if (CpuBuffer == null) {
            throw new Exception("8q3y4tog");
        }

        ClContext context = _context.Get<ClContext>();

        GpuBuffer ??= new ClBuffer(context, CpuBuffer.SizeInBytes);

        var commandQueue = new ClCommandQueue(context);
        commandQueue.EnqueueWriteBuffer(GpuBuffer, CpuBuffer.Ptr);
        commandQueue.Finish();
    }

    private void CopyToCpu() {
        if (GpuBuffer == null) {
            throw new Exception("8q3343y4tog");
        }

        ClContext context = _context.Get<ClContext>();

        CpuBuffer ??= new MemoryBuffer(SizeInBytes);
        var commandQueue = new ClCommandQueue(context);
        commandQueue.EnqueueReadBuffer(GpuBuffer, CpuBuffer.Ptr);
        commandQueue.Finish();
    }

    public BitmapSource ConvertToBitmapSource() {
        if (CpuBuffer == null) {
            throw new Exception("8e7ug8rfurl");
        }

        return BitmapSource.Create(
            Width,
            Height,
            1.0,
            1.0,
            PixelFormatInfo.Wmpf,
            BitmapPalettes.Gray256,
            CpuBuffer.Ptr,
            SizeInBytes,
            Stride
        );
    }
}