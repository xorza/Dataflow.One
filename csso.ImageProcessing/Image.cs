using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using csso.Common;
using csso.OpenCL;
using PixelFormat = csso.Common.PixelFormat;

namespace csso.ImageProcessing;

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


    public UInt32 Height { get; }
    public UInt32 Width { get; }
    public UInt32 Stride { get; }
    public UInt32 SizeInBytes { get; }
    public PixelFormatInfo PixelFormatInfo { get; }

    public Image(Context ctx, PixelFormat pf, UInt32 width, UInt32 height) {
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

        IntPtr data;

        using (var fileStream = fileInfo.OpenRead())
        using (var bitmap = new Bitmap(fileStream)) {
            Height = (UInt32) bitmap.Height;
            Width = (UInt32) bitmap.Width;
            PixelFormatInfo = PixelFormatInfo.Get(bitmap.PixelFormat);

            BitmapData? bitmapData = null;
            try {
                bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    bitmap.PixelFormat);

                Stride = (UInt32) bitmapData.Stride;
                SizeInBytes = Stride * Height;

                data = Memory.Alloc(SizeInBytes);

                Buffer.MemoryCopy(
                    bitmapData.Scan0.ToPointer(),
                    data.ToPointer(),
                    SizeInBytes,
                    SizeInBytes);
            }
            finally {
                if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }
        }


        _cpuBuffer = new MemoryBuffer(data, SizeInBytes, false);

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

        if (!_isGpuBufferDirty) {
            return;
        }

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

        if (!_isCpuBufferDirty) {
            return;
        }

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
            (Int32) Width,
            (Int32) Height,
            1.0,
            1.0,
            PixelFormatInfo.Wmpf,
            BitmapPalettes.Gray256,
            buffer.Ptr,
            (Int32) SizeInBytes,
            (Int32) (Width * PixelFormatInfo.BytesPerPixel)
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

        for (UInt32 row = 0; row < Height; row++) {
            for (UInt32 column = 0; column < Width; column++) {
                UInt32 offset = (UInt32) (row * Stride + column * sizeof(T));
                _cpuBuffer.Set(offset, pixels[row * Width + column]);
            }
        }

        _isCpuBufferDirty = false;
        _isGpuBufferDirty = true;
    }

    public T Get<T>(UInt32 w, UInt32 h) where T : unmanaged {
        if (_cpuBuffer == null) {
            throw new Exception("y983g4qhvead");
        }

        UInt32 offset = (UInt32) (h * Stride + w * sizeof(T));
        return _cpuBuffer.Get<T>(offset);
    }
}