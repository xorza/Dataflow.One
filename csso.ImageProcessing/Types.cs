using System;

namespace csso.ImageProcessing {
public interface ICastFromUnmanaged {
    void Cast(IntPtr ptr);
}

public struct RGB8U : ICastFromUnmanaged {
    public byte r;
    public byte g;
    public byte b;

    // public RGB8U(RGB32F rgb32f) {
    //     
    // }
    public unsafe void Cast(IntPtr ptr) {
        RGB8U* val = (RGB8U*) ptr.ToPointer();
        r = val->r;
        g = val->g;
        b = val->b;
    }
}

public struct RGB32F : ICastFromUnmanaged {
    public float r;
    public float g;
    public float b;

    public RGB32F(RGB8U rgb8u) {
        r = rgb8u.r;
        g = rgb8u.g;
        b = rgb8u.b;
    }

    public unsafe void Cast(IntPtr ptr) {
        RGB32F* val = (RGB32F*) ptr.ToPointer();
        r = val->r;
        g = val->g;
        b = val->b;
    }
}
}