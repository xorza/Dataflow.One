using System;

namespace csso.Common;

public enum PixelFormat {
    Rgb8,
    Rgba8
}

public static class PixelFormatXtenstions {
    private const UInt32 MemoryAlignment = 64;
    
    public static UInt32 CalculateStride(this PixelFormat pf, UInt32 width) {
        var bytesPerPixel = BytesPerPixel(pf);
        var bytesPerRow = bytesPerPixel * width;
        var stride = (bytesPerRow + (MemoryAlignment - 1)) & ~(MemoryAlignment - 1);
        return stride;
    }

    public static UInt32 ChannelCount(this PixelFormat pf) {
        switch (pf) {
            case PixelFormat.Rgb8:
                return 3;
            case PixelFormat.Rgba8:
                return 4;
        }

        throw new Exception("ch4556d");
    }

    public static UInt32 BytesPerChannel(this PixelFormat pf) {
        switch (pf) {
            case PixelFormat.Rgb8:
            case PixelFormat.Rgba8:
                return 1;
        }

        throw new Exception("sqv54y545");
    }

    public static UInt32 BytesPerPixel(this PixelFormat pf) {
        return ChannelCount(pf) * BytesPerChannel(pf);
    }
}