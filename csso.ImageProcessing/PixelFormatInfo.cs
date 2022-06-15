using System;
using System.Linq;
using csso.Common;
using DrawingImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = csso.Common.PixelFormat;
using WindowsMediaPixelFormats = System.Windows.Media.PixelFormats;
using WindowsMediaPixelFormat = System.Windows.Media.PixelFormat;


namespace csso.ImageProcessing;

public class PixelFormatInfo {
    private static readonly PixelFormatInfo[] pixelFormats =
        new PixelFormatInfo[] {
            new PixelFormatInfo() {
                Dipf = DrawingImagingPixelFormat.Format32bppArgb,
                Wmpf = WindowsMediaPixelFormats.Bgra32,
                Pf = PixelFormat.Rgba8,
                ChannelCount = PixelFormat.Rgba8.ChannelCount(),
                BytesPerChannel = PixelFormat.Rgba8.BytesPerChannel(),
                BytesPerPixel = PixelFormat.Rgba8.BytesPerPixel()
            },
            new PixelFormatInfo() {
                Dipf = DrawingImagingPixelFormat.Format24bppRgb,
                Wmpf = WindowsMediaPixelFormats.Bgr32,
                Pf = PixelFormat.Rgb8,
                ChannelCount = PixelFormat.Rgb8.ChannelCount(),
                BytesPerChannel = PixelFormat.Rgb8.BytesPerChannel(),
                BytesPerPixel = PixelFormat.Rgb8.BytesPerPixel()
            }
        };

    public static PixelFormatInfo Get(PixelFormat pf) {
        return pixelFormats.Single(_ => _.Pf == pf);
    }
    public static PixelFormatInfo Get(DrawingImagingPixelFormat dipf) {
        return pixelFormats.Single(_ => _.Dipf == dipf);
    }

    public PixelFormat Pf { get; private set; }
    public DrawingImagingPixelFormat Dipf { get; private set; }
    public WindowsMediaPixelFormat Wmpf { get; private set; }
    public UInt32 ChannelCount { get; private set; }
    public UInt32 BytesPerChannel { get; private set; }
    public UInt32 BytesPerPixel { get; private set; }

    private PixelFormatInfo() { }

    public UInt32 CalculateStride(UInt32 width) {
        return Pf.CalculateStride(width);
    }
}