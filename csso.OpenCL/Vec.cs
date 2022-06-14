using System;

namespace csso.OpenCL;

public struct Vec2d {
    public double x;
    public double y;
}

public struct Vec4b {
    public byte x;
    public byte y;
    public byte z;
    public byte w;

    public Vec4b(byte x, byte y, byte z, byte w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    
    public Vec4b(Vec3b v3, byte w) {
        this.x = v3.x;
        this.y = v3.y;
        this.z = v3.z;
        this.w = w;
    }
}


public struct Vec3b {
    public byte x;
    public byte y;
    public byte z;

    public Vec3b(byte x, byte y, byte z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public struct Vec2f {
    public float x;
    public float y;
}