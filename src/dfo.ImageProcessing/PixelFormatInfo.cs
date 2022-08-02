using System.Linq;
using dfo.Common;
using DrawingImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using WindowsMediaPixelFormats = System.Windows.Media.PixelFormats;
using WindowsMediaPixelFormat = System.Windows.Media.PixelFormat;


namespace dfo.ImageProcessing;

public class PixelFormatInfo {
    private static readonly PixelFormatInfo[] PixelFormats = {
        new() {
            Dipf = DrawingImagingPixelFormat.Format32bppArgb,
            Wmpf = WindowsMediaPixelFormats.Bgra32,
            Pf = PixelFormat.Rgba8,
            ChannelCount = PixelFormat.Rgba8.ChannelCount(),
            BytesPerChannel = PixelFormat.Rgba8.BytesPerChannel(),
            BytesPerPixel = PixelFormat.Rgba8.BytesPerPixel()
        }
    };

    private PixelFormatInfo() { }

    public PixelFormat Pf { get; private set; }
    public DrawingImagingPixelFormat Dipf { get; private set; }
    public WindowsMediaPixelFormat Wmpf { get; private set; }
    public uint ChannelCount { get; private set; }
    public uint BytesPerChannel { get; private set; }
    public uint BytesPerPixel { get; private set; }

    public static PixelFormatInfo Get(PixelFormat pf) {
        return PixelFormats.Single(_ => _.Pf == pf);
    }

    public static PixelFormatInfo Get(DrawingImagingPixelFormat dipf) {
        return PixelFormats.Single(_ => _.Dipf == dipf);
    }

    public uint CalculateStride(uint width) {
        return Pf.CalculateStride(width);
    }
}