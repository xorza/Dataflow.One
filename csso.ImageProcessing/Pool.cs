namespace csso.ImageProcessing;

public class ImagePool {
    private readonly Context _context;

    public ImagePool(Context ctx) {
        _context = ctx;
    }

    public Image Acquire(int width, int height, PixelFormat pixelFormat) {
        return new Image(_context, width, height, pixelFormat);
    }
}