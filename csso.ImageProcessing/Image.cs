using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using csso.OpenCL;
using DrawingImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using WindowsMediaPixelFormat = System.Windows.Media.PixelFormat;

namespace csso.ImageProcessing;

public enum PixelFormat {
    Rgb8,
    Rgba8
}

public class PixelFormatInfo {
    public static readonly PixelFormatInfo[] All = {
        new() {
            Dipf = DrawingImagingPixelFormat.Format32bppArgb,
            Wmpf = PixelFormats.Bgra32,
            Pf = PixelFormat.Rgba8,
            ChannelCount = 4,
            BytesPerChannel = 1
        },
        new() {
            Dipf = DrawingImagingPixelFormat.Format24bppRgb,
            Wmpf = PixelFormats.Bgr24,
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

    private PixelFormatInfo() { }
}

public class Image : IDisposable {
    public enum Operation {
        Read,
        Write
    }

    private const int StrideAlignment = 8;

    private readonly Context _context;
    private MemoryBuffer? _cpuBuffer;
    private ClBuffer? _gpuBuffer;
    private bool _isCpuBufferDirty = true;
    private bool _isGpuBufferDirty = true;

    public Image(Context ctx,
        int width, int height,
        PixelFormat pixelFormat) {
        _context = ctx;
        Width = width;
        Height = height;
        var pixelFormatInfo = PixelFormatInfo.Get(pixelFormat);
        PixelFormatInfo = pixelFormatInfo;
        var bytesPerRow = width * pixelFormatInfo.BytesPerChannel * pixelFormatInfo.ChannelCount;

        Stride = (bytesPerRow + (StrideAlignment - 1)) & ~(StrideAlignment - 1);
        SizeInBytes = height * Stride;

        _isCpuBufferDirty = true;
        _isGpuBufferDirty = true;
    }

    public unsafe Image(Context ctx,
        int width, int height,
        Vec4b[] pixels,
        PixelFormat pixelFormat)
        : this(ctx,
            width,
            height,
            pixelFormat) {
        if (pixels.Length != width * height) {
            throw new ArgumentException(nameof(pixels));
        }

        _cpuBuffer = new MemoryBuffer(SizeInBytes);

        for (int i = 0; i < pixels.Length; i++) {
            _cpuBuffer.Set(i, pixels[i]);
        }

        _isCpuBufferDirty = false;
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
                _cpuBuffer = new MemoryBuffer((IntPtr) imageData.Scan0.ToPointer(), SizeInBytes, true);
            }
        }
        finally {
            if (imageData != null) img.UnlockBits(imageData);

            img.Dispose();
        }

        _isCpuBufferDirty = false;
        _isGpuBufferDirty = true;
    }


    public int Height { get; }
    public int Width { get; }
    public int Stride { get; }
    public int SizeInBytes { get; }
    public PixelFormatInfo PixelFormatInfo { get; }

    public void Dispose() {
        _gpuBuffer?.Dispose();
        _cpuBuffer?.Dispose();
    }

    private void CopyToGpu() {
        if (_cpuBuffer == null) throw new Exception("w4w4vywrts");

        if (_isCpuBufferDirty) throw new Exception("wy455w4h5rh");


        var context = _context.Get<ClContext>();

        _gpuBuffer ??= new ClBuffer(context, SizeInBytes);

        var commandQueue = new ClCommandQueue(context);
        commandQueue.EnqueueWriteBuffer(_gpuBuffer, _cpuBuffer.Ptr);
        commandQueue.Finish();

        _isGpuBufferDirty = false;
    }

    private void CopyToCpu() {
        if (_gpuBuffer == null) throw new Exception("8q3343y4tog");

        if (_isGpuBufferDirty) throw new Exception("w4vy545y");

        var context = _context.Get<ClContext>();

        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);
        var commandQueue = new ClCommandQueue(context);
        commandQueue.EnqueueReadBuffer(_gpuBuffer, _cpuBuffer.Ptr);
        commandQueue.Finish();

        _isCpuBufferDirty = false;
    }

    public BitmapSource ConvertToBitmapSource() {
        var buffer = TakeCpuBuffer(Operation.Read);

        return BitmapSource.Create(
            Width,
            Height,
            1.0,
            1.0,
            PixelFormatInfo.Wmpf,
            BitmapPalettes.Gray256,
            buffer.Ptr,
            SizeInBytes,
            Stride
        );
    }

    public ClBuffer TakeGpuBuffer(Operation op) {
        var clContext = _context.Get<ClContext>();

        _gpuBuffer ??= new ClBuffer(clContext, SizeInBytes);

        if (_isGpuBufferDirty && op == Operation.Read) CopyToGpu();

        if (op == Operation.Write) {
            _isCpuBufferDirty = true;
            _isGpuBufferDirty = false;
        }

        return _gpuBuffer!;
    }

    public MemoryBuffer TakeCpuBuffer(Operation op) {
        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);

        if (_isCpuBufferDirty && op == Operation.Read)
            CopyToCpu();

        if (op == Operation.Write) {
            _isGpuBufferDirty = true;
            _isCpuBufferDirty = false;
        }

        return _cpuBuffer!;
    }
}