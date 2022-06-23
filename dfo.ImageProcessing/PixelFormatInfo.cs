using System;
using System.Linq;
using dfo.Common;
using DrawingImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = dfo.Common.PixelFormat;
using WindowsMediaPixelFormats = System.Windows.Media.PixelFormats;
using WindowsMediaPixelFormat = System.Windows.Media.PixelFormat;


namespace dfo.ImageProcessing;

public class PixelFormatInfo {
    private static readonly PixelFormatInfo[] PixelFormats =
        new PixelFormatInfo[] {
            new PixelFormatInfo() {
                Dipf = DrawingImagingPixelFormat.Format32bppArgb,
                Wmpf = WindowsMediaPixelFormats.Bgra32,
                Pf = PixelFormat.Rgba8,
                ChannelCount = PixelFormat.Rgba8.ChannelCount(),
                BytesPerChannel = PixelFormat.Rgba8.BytesPerChannel(),
                BytesPerPixel = PixelFormat.Rgba8.BytesPerPixel()
            }
        };

    public static PixelFormatInfo Get(PixelFormat pf) {
        return PixelFormats.Single(_ => _.Pf == pf);
    }
    public static PixelFormatInfo Get(DrawingImagingPixelFormat dipf) {
        return PixelFormats.Single(_ => _.Dipf == dipf);
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