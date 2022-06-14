using System;
using System.Diagnostics;
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
    public static readonly PixelFormatInfo Rgba =
        new() {
            Dipf = DrawingImagingPixelFormat.Format32bppArgb,
            Wmpf = PixelFormats.Bgra32,
            Pf = PixelFormat.Rgba8,
            ChannelCount = 4,
            BytesPerChannel = 1
        };

    public PixelFormat Pf { get; private set; }
    public DrawingImagingPixelFormat Dipf { get; private set; }
    public WindowsMediaPixelFormat Wmpf { get; private set; }
    public int ChannelCount { get; private set; }
    public int BytesPerChannel { get; private set; }
    public int BytesPerPixel => ChannelCount * BytesPerChannel;

    private PixelFormatInfo() { }
}

public unsafe class Image : IDisposable {
    public enum Operation {
        Read,
        Write
    }

    private readonly Context _context;
    private MemoryBuffer? _cpuBuffer;
    private ClBuffer? _gpuBuffer;
    private bool _isCpuBufferDirty = true;
    private bool _isGpuBufferDirty = true;


    public int Height { get; }
    public int Width { get; }
    public int SizeInBytes => Height * Width * PixelFormatInfo.BytesPerPixel;
    public PixelFormatInfo PixelFormatInfo => PixelFormatInfo.Rgba;

    public Image(Context ctx, int width, int height) {
        _context = ctx;
        Width = width;
        Height = height;

        _isCpuBufferDirty = true;
        _isGpuBufferDirty = true;
    }

    public Image(Context ctx, FileInfo fileInfo) {
        _context = ctx;

        IntPtr rgba = IntPtr.Zero;

        using (var fileStream = fileInfo.OpenRead())
        using (var bitmap = new Bitmap(fileStream)) {
            Height = bitmap.Height;
            Width = bitmap.Width;

            BitmapData? imageData = null;

            try {
                imageData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    bitmap.PixelFormat);

                rgba = ToRgba(imageData);
            }
            finally {
                if (imageData != null) bitmap.UnlockBits(imageData);
            }
        }


        _cpuBuffer = new MemoryBuffer(rgba, SizeInBytes, false);

        _isCpuBufferDirty = false;
        _isGpuBufferDirty = true;
    }

    private IntPtr ToRgba(BitmapData bitmapData) {
        IntPtr result = Marshal.AllocHGlobal(SizeInBytes);

        if (bitmapData.PixelFormat == DrawingImagingPixelFormat.Format32bppArgb) {
            Buffer.MemoryCopy(
                bitmapData.Scan0.ToPointer(),
                result.ToPointer(),
                SizeInBytes,
                SizeInBytes);
            return result;
        }

        if (bitmapData.PixelFormat == DrawingImagingPixelFormat.Format24bppRgb) {
            Vec4b* dest = (Vec4b*) result.ToPointer();
            Vec3b* source = (Vec3b*) bitmapData.Scan0.ToPointer();

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    dest[j * Width + i] = new Vec4b(source[j * Width + i], 255);
                }
            }

            return result;
        }

        throw new Exception("ag8w9ey9qerhv");
    }

    public void Dispose() {
        _gpuBuffer?.Dispose();
        _cpuBuffer?.Dispose();
    }

    public void UpdateGpuBuffer() {
        if (_cpuBuffer == null) throw new Exception("w4w4vywrts");

        if (_isCpuBufferDirty) throw new Exception("wy455w4h5rh");

        if (!_isGpuBufferDirty) {
            return;
        }

        var context = _context.Get<ClContext>();

        _gpuBuffer ??= new ClBuffer(context, SizeInBytes);

        var commandQueue = new ClCommandQueue(context);
        commandQueue.EnqueueWriteBuffer(_gpuBuffer, _cpuBuffer.Ptr);
        commandQueue.Finish();

        _isGpuBufferDirty = false;
    }

    public void UpdateCpuBuffer() {
        if (_gpuBuffer == null) throw new Exception("8q3343y4tog");

        if (_isGpuBufferDirty) throw new Exception("w4vy545y");

        if (!_isCpuBufferDirty) {
            return;
        }

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
            Width * PixelFormatInfo.BytesPerPixel
        );
    }

    public ClBuffer TakeGpuBuffer(Operation op) {
        var clContext = _context.Get<ClContext>();

        _gpuBuffer ??= new ClBuffer(clContext, SizeInBytes);

        if (_isGpuBufferDirty && op == Operation.Read) UpdateGpuBuffer();

        if (op == Operation.Write) {
            _isCpuBufferDirty = true;
            _isGpuBufferDirty = false;
        }

        return _gpuBuffer!;
    }

    public MemoryBuffer TakeCpuBuffer(Operation op) {
        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);

        if (_isCpuBufferDirty && op == Operation.Read)
            UpdateCpuBuffer();

        if (op == Operation.Write) {
            _isGpuBufferDirty = true;
            _isCpuBufferDirty = false;
        }

        return _cpuBuffer!;
    }

    public void Set<T>(T[] pixels) where T : unmanaged {
        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);

        for (int row = 0; row < Height; row++) {
            for (int column = 0; column < Width; column++) {
                Int32 offset = row * Width * PixelFormatInfo.BytesPerPixel + column * sizeof(T);
                _cpuBuffer.Set(offset, pixels[column]);
            }
        }

        _isCpuBufferDirty = false;
        _isGpuBufferDirty = true;
    }

    public T Get<T>(int w, int h) where T : unmanaged {
        if (_cpuBuffer == null) {
            throw new Exception("y983g4qhvead");
        }

        Int32 offset = h * Width * PixelFormatInfo.BytesPerPixel + w * sizeof(T);
        return _cpuBuffer.Get<T>(offset);
    }
}