using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;

namespace csso.ImageProcessing {
public class Image : IDisposable {
    public Int32 Height { get; private set; } = 0;
    public Int32 Width { get; private set; } = 0;
    public Int32 Stride { get; private set; } = 0;
    public Int32 ChannelCount { get; private set; }
    public Int32 BytesPerChannel { get; private set; }
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

    private void SetPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat) {
        switch (pixelFormat) {
            case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                ChannelCount = 3;
                BytesPerChannel = 1;
                break;

            default:
                throw new Exception(nameof(PixelFormat));
        }
    }

    public unsafe T Get<T>(Int32 x, Int32 y) where T : unmanaged {
        T* data = (T*) (_ptr + y * Stride).ToPointer();
        return data[x];
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
}