using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace csso.ImageProcessing {
public class Image : IDisposable {
    private IntPtr _ptr;

    public Image(string filename) {
        var img = new Bitmap(filename);
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
        }
        finally {
            if (imageData != null)
                img.UnlockBits(imageData);

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

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

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
        T[] result = new T[TotalPixels];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            result[y * Width + x] = Get<T>(x, y);

        return result;
    }


    private void ReleaseUnmanagedResources() {
        if (_ptr != IntPtr.Zero) {
            Marshal.FreeHGlobal(_ptr);
            _ptr = IntPtr.Zero;
        }
    }

    ~Image() {
        ReleaseUnmanagedResources();
    }
}
}