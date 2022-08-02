using dfo.Common;

namespace dfo.ImageProcessing;

public class ImagePool {
    private readonly Context _context;

    public ImagePool(Context ctx) {
        _context = ctx;
    }

    public Image Acquire(uint width, uint height) {
        return new Image(_context, PixelFormat.Rgba8, width, height);
    }
}