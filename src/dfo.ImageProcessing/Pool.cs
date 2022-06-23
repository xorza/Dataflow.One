using System;
using dfo.Common;

namespace dfo.ImageProcessing;

public class ImagePool {
    private readonly Context _context;

    public ImagePool(Context ctx) {
        _context = ctx;
    }

    public Image Acquire(UInt32 width, UInt32 height) {
        return new Image(_context, PixelFormat.Rgba8, width, height);
    }
}