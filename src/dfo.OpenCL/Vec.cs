using System.Runtime.InteropServices;

namespace dfo.OpenCL;

[StructLayout(LayoutKind.Sequential)]
public struct Vec2d {
    public double x;
    public double y;
}

[StructLayout(LayoutKind.Sequential)]
public struct Vec4b {
    public byte x;
    public byte y;
    public byte z;
    public byte w;

    public Vec4b(byte v) {
        x = v;
        y = v;
        z = v;
        w = v;
    }

    public Vec4b(byte x, byte y, byte z, byte w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Vec4b(Vec3b v3, byte w) {
        x = v3.x;
        y = v3.y;
        z = v3.z;
        this.w = w;
    }

    public override string ToString() {
        return $"{{ {x}, {y}, {z}, {w} }}";
    }
}

[StructLayout(LayoutKind.Sequential)]
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

[StructLayout(LayoutKind.Sequential)]
public struct Vec2f {
    public float x;
    public float y;
}