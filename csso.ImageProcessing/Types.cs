using System;

namespace csso.ImageProcessing {
public struct RGB8U {
    public byte r;
    public byte g;
    public byte b;

    public RGB8U(RGB16U other) {
        r = (byte) (other.r / 256);
        g = (byte) (other.g / 256);
        b = (byte) (other.b / 256);
    }
}

public struct RGBA8U {
    public byte r;
    public byte g;
    public byte b;
    public byte a;

    public RGBA8U(RGBA16U other) {
        r = (byte) (other.r / 256);
        g = (byte) (other.g / 256);
        b = (byte) (other.b / 256);
        a = (byte) (other.a / 256);
    }
}

public struct RGB16U {
    public UInt16 r;
    public UInt16 g;
    public UInt16 b;

    public RGB16U(RGB8U other) {
        r = (UInt16) (other.r * 256);
        g = (UInt16) (other.g * 256);
        b = (UInt16) (other.b * 256);
    }
}

public struct RGBA16U {
    public UInt16 r;
    public UInt16 g;
    public UInt16 b;
    public UInt16 a;

    public RGBA16U(RGBA8U other) {
        r = (UInt16) (other.r * 256);
        g = (UInt16) (other.g * 256);
        b = (UInt16) (other.g * 256);
        a = (UInt16) (other.a * 256);
    }
}

public struct RGB32F {
    public float r;
    public float g;
    public float b;

    public RGB32F(RGB8U other) {
        r = other.r;
        g = other.g;
        b = other.b;
    }
}
}