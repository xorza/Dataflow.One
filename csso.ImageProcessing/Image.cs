using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using csso.OpenCL;

namespace csso.ImageProcessing;

public class Image : IDisposable {
    private IntPtr _ptr;

    public Image(string filename) : this(new FileInfo(filename)) { }

    public Image(FileInfo fileInfo) {
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

            SetPixelFormat(imageData.PixelFormat);
            SizeInBytes = imageData.Height * imageData.Stride;
            Height = imageData.Height;
            Width = imageData.Width;
            Stride = imageData.Stride;

            _ptr = Marshal.AllocHGlobal(SizeInBytes);
            unsafe {
                Buffer.MemoryCopy(imageData.Scan0.ToPointer(), _ptr.ToPointer(), SizeInBytes, SizeInBytes);
            }
        } finally {
            if (imageData != null) {
                img.UnlockBits(imageData);
            }

            img.Dispose();
        }
    }

    public int Height { get; }
    public int Width { get; }
    public int Stride { get; }
    public int ChannelCount { get; private set; }
    public int BytesPerChannel { get; private set; }
    public int SizeInBytes { get; }
    public int TotalPixels => Height * Width;

    private void SetPixelFormat(PixelFormat pixelFormat) {
        switch (pixelFormat) {
            case PixelFormat.Format24bppRgb:
                ChannelCount = 3;
                BytesPerChannel = 1;
                break;

            default:
                throw new Exception(nameof(PixelFormat));
        }
    }

    public unsafe T Get<T>(int x, int y) where T : unmanaged {
        var data = (T*) (_ptr + y * Stride).ToPointer();
        return data[x];
    }

    public T[] As<T>() where T : unmanaged {
        var result = new T[TotalPixels];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            result[y * Width + x] = Get<T>(x, y);

        return result;
    }

    public ClBuffer CreateBuffer(csso.OpenCL.ClContext clContext) {
        var pixelCount = TotalPixels;

        var pixels8U = As<RGB8U>();
        var pixels16U = new RGB16U[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            pixels16U[i] = new RGB16U(pixels8U[i]);

        var resultPixels = new RGB16U[pixelCount];

        var a = ClBuffer.Create(clContext, pixels16U);
        return a;
    }


    private void ReleaseUnmanagedResources() {
        if (_ptr != IntPtr.Zero) {
            Marshal.FreeHGlobal(_ptr);
            _ptr = IntPtr.Zero;
        }
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Image() {
        ReleaseUnmanagedResources();
    }
}