using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;

namespace csso.ImageProcessing {
public enum PixelFormat : Int32 {
    R8,
    R16,
    RGB8,
    RGBA8,
    RGB16,
    RGBA16,
}

public class Image : IDisposable {
    public Int32 Stride { get; private set; } = 0;
    public Int32 Height { get; private set; } = 0;
    public Int32 Width { get; private set; } = 0;
    public PixelFormat PixelFormat { get; private set; }
    public Int32 ChannelCount => GetComponentCount();
    public Int32 BytesPerChannel => GetBytesPerChannel();
    public Int32 SizeInBytes { get; }
    public Int32 TotalPixels => Height * Width;

    private IntPtr _ptr;

    public Image(String filename) {
        var img = new Bitmap(filename);
        BitmapData? imageData = null;

        try {
            imageData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly,
                img.PixelFormat);

            SizeInBytes = imageData.Width * img.Height * BytesPerChannel * ChannelCount;
            Stride = imageData.Stride;
            Height = imageData.Height;
            Width = imageData.Width;
            PixelFormat = ConvertPixelFormat(imageData.PixelFormat);

            _ptr = Marshal.AllocHGlobal(SizeInBytes);
            IntPtr ptr = imageData.Scan0;

            unsafe {
                for (int y = 0; y < Height; y++) {
                    Buffer.MemoryCopy(
                        (ptr + y * Stride).ToPointer(),
                        (_ptr + y * Width).ToPointer(),
                        Width,
                        Width
                    );
                }
            }
        }
        finally {
            if (imageData != null)
                img.UnlockBits(imageData);

            img.Dispose();
        }
    }

    private PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat) {
        switch (pixelFormat) {
            case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                return PixelFormat.RGB8;

            default:
                throw new Exception(nameof(PixelFormat));
        }
    }

    private Int32 GetComponentCount() {
        switch (PixelFormat) {
            case PixelFormat.R8:
            case PixelFormat.R16:
                return 1;
            case PixelFormat.RGB8:
            case PixelFormat.RGB16:
                return 3;
            case PixelFormat.RGBA8:
            case PixelFormat.RGBA16:
                return 4;

            default:
                throw new Exception("");
        }
    }

    private Int32 GetBytesPerChannel() {
        switch (PixelFormat) {
            case PixelFormat.R8:
            case PixelFormat.RGB8:
            case PixelFormat.RGBA8:
                return 1;
            case PixelFormat.R16:
            case PixelFormat.RGB16:
            case PixelFormat.RGBA16:
                return 2;

            default:
                throw new Exception("");
        }
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

    public T Get<T>(Int32 x, Int32 y) where T : unmanaged {
        unsafe {
            T* data = (T*) (_ptr + y * Width).ToPointer();
            return data[x];
        }
    }

    public T[] As<T>() where T : unmanaged {
        T[] result = new T[TotalPixels];
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                result[y * Width + x] = Get<T>(x, y);
            }
        }

        return result;
    }

    ~Image() {
        ReleaseUnmanagedResources();
    }
}
}