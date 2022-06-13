using System;
using System.Collections.Generic;

namespace csso.ImageProcessing;

public class ImagePool {
    private readonly Context _context;

    public ImagePool(Context ctx) {
        _context = ctx;
    }

    public Image Acquire(Int32 width, Int32 height, PixelFormat pixelFormat) {
        return new Image(_context, width, height, pixelFormat);
    }
}