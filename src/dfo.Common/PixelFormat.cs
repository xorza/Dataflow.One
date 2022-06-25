using System;

namespace dfo.Common;

public enum PixelFormat {
    // Rgb8, - not supported by GPU hardware
    Rgba8
}

public static class PixelFormatXtenstions {
    private const uint MemoryAlignment = 8;

    public static uint CalculateStride(this PixelFormat pf, uint width) {
        var bytesPerPixel = BytesPerPixel(pf);
        var bytesPerRow = bytesPerPixel * width;
        var stride = (bytesPerRow + (MemoryAlignment - 1)) & ~(MemoryAlignment - 1);
        return stride;
    }

    public static uint ChannelCount(this PixelFormat pf) {
        switch (pf) {
            case PixelFormat.Rgba8:
                return 4;
        }

        throw new Exception("ch4556d");
    }

    public static uint BytesPerChannel(this PixelFormat pf) {
        switch (pf) {
            case PixelFormat.Rgba8:
                return 1;
        }

        throw new Exception("sqv54y545");
    }

    public static uint BytesPerPixel(this PixelFormat pf) {
        return ChannelCount(pf) * BytesPerChannel(pf);
    }
}