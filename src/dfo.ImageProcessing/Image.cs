using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using dfo.Common;
using dfo.OpenCL;
using PixelFormat = dfo.Common.PixelFormat;

namespace dfo.ImageProcessing;

public unsafe class Image : IDisposable {
    public enum Operation {
        Read,
        Write
    }

    private readonly Context _context;
    private MemoryBuffer? _cpuBuffer;
    private ClImage? _gpuBuffer;
    private bool _isCpuBufferDirty = true;
    private bool _isGpuBufferDirty = true;


    public uint Height { get; }
    public uint Width { get; }
    public uint Stride { get; }
    public uint SizeInBytes { get; }
    public PixelFormatInfo PixelFormatInfo { get; }

    public Image(Context ctx, PixelFormat pf, uint width, uint height) {
        _context = ctx;

        PixelFormatInfo = PixelFormatInfo.Get(pf);
        Width = width;
        Height = height;
        Stride = PixelFormatInfo.CalculateStride(Width);
        SizeInBytes = Height * Stride;

        _isCpuBufferDirty = true;
        _isGpuBufferDirty = true;
    }

    public Image(Context ctx, FileInfo fileInfo) {
        _context = ctx;

        using var fileStream = fileInfo.OpenRead();
        using var bitmap = new Bitmap(fileStream);

        Height = (uint) bitmap.Height;
        Width = (uint) bitmap.Width;
        switch (bitmap.PixelFormat) {
            case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
            case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                PixelFormatInfo = PixelFormatInfo.Get(PixelFormat.Rgba8);
                break;
            default:
                throw new Exception("aeoihrogpq98354");
        }

        Stride = PixelFormatInfo.CalculateStride(Width);
        SizeInBytes = Height * Stride;


        BitmapData? bitmapData = null;
        _cpuBuffer = new MemoryBuffer(SizeInBytes);

        try {
            bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            var bmpData = bitmapData.Scan0;

            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                for (uint row = 0; row < Height; row++) {
                    var rgbRow = (Vec3b*) (bmpData + (int) (row * bitmapData.Stride)).ToPointer();
                    var rgbaRow = (Vec4b*) (_cpuBuffer.Ptr + (int) (row * Stride)).ToPointer();

                    for (uint column = 0; column < Width; column++) rgbaRow[column] = new Vec4b(rgbRow[column], 255);
                }

            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                _cpuBuffer.Upload(bitmapData.Scan0, 0, (uint) (bitmapData.Stride * bitmapData.Height));
        }
        finally {
            if (bitmapData != null) bitmap.UnlockBits(bitmapData);
        }

        _isCpuBufferDirty = false;
        _isGpuBufferDirty = true;
    }


    public void Dispose() {
        _gpuBuffer?.Dispose();
        _cpuBuffer?.Dispose();
    }

    private ClImage CreateGpuBuffer(ClContext ctx) {
        _isGpuBufferDirty = _isCpuBufferDirty;
        return new ClImage(ctx, Width, Height, Stride, PixelFormatInfo.Pf, _cpuBuffer);
    }

    public void UpdateGpuBuffer() {
        if (_cpuBuffer == null) throw new Exception("w4w4vywrts");

        if (_isCpuBufferDirty) throw new Exception("wy455w4h5rh");

        if (!_isGpuBufferDirty) return;

        var context = _context.Get<ClContext>();

        _gpuBuffer ??= CreateGpuBuffer(context);

        using var commandQueue = new ClCommandQueue(context);
        _gpuBuffer.Upload(commandQueue, _cpuBuffer);
        commandQueue.Finish();

        _isGpuBufferDirty = false;
    }

    public void UpdateCpuBuffer() {
        if (_gpuBuffer == null) throw new Exception("8q3343y4tog");

        if (_isGpuBufferDirty) throw new Exception("w4vy545y");

        if (!_isCpuBufferDirty) return;

        var context = _context.Get<ClContext>();

        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);
        using var commandQueue = new ClCommandQueue(context);
        _gpuBuffer.Download(commandQueue, _cpuBuffer);
        commandQueue.Finish();

        _isCpuBufferDirty = false;
    }

    public BitmapSource ConvertToBitmapSource() {
        var buffer = TakeCpuBuffer(Operation.Read);

        return BitmapSource.Create(
            (int) Width,
            (int) Height,
            1.0,
            1.0,
            PixelFormatInfo.Wmpf,
            BitmapPalettes.Gray256,
            buffer.Ptr,
            (int) SizeInBytes,
            (int) (Width * PixelFormatInfo.BytesPerPixel)
        );
    }

    public ClImage TakeGpuBuffer(Operation op) {
        var clContext = _context.Get<ClContext>();

        _gpuBuffer ??= CreateGpuBuffer(clContext);

        if (_isGpuBufferDirty && op == Operation.Read) UpdateGpuBuffer();

        if (op == Operation.Write) {
            _isCpuBufferDirty = true;
            _isGpuBufferDirty = false;
        }

        return _gpuBuffer;
    }

    public MemoryBuffer TakeCpuBuffer(Operation op) {
        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);

        if (_isCpuBufferDirty && op == Operation.Read)
            UpdateCpuBuffer();

        if (op == Operation.Write) {
            _isGpuBufferDirty = true;
            _isCpuBufferDirty = false;
        }

        return _cpuBuffer;
    }

    public void Set<T>(T[] pixels) where T : unmanaged {
        _cpuBuffer ??= new MemoryBuffer(SizeInBytes);

        for (uint row = 0; row < Height; row++)
        for (uint column = 0; column < Width; column++) {
            var offset = (uint) (row * Stride + column * sizeof(T));
            _cpuBuffer.Set(offset, pixels[row * Width + column]);
        }

        _isCpuBufferDirty = false;
        _isGpuBufferDirty = true;
    }

    public T Get<T>(uint w, uint h) where T : unmanaged {
        if (_cpuBuffer == null) throw new Exception("y983g4qhvead");

        var offset = (uint) (h * Stride + w * sizeof(T));
        return _cpuBuffer.Get<T>(offset);
    }
}