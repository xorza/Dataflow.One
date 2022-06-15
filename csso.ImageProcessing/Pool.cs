using System;
using csso.Common;

namespace csso.ImageProcessing;

public class ImagePool {
    private readonly Context _context;

    public ImagePool(Context ctx) {
        _context = ctx;
    }

    public Image Acquire(PixelFormat pf, UInt32 width, UInt32 height) {
        return new Image(_context, pf, width, height);
    }
}