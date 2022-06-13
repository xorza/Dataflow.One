using System;
using System.Linq;
using csso.ImageProcessing.Funcs;
using csso.OpenCL;
using NUnit.Framework;

namespace csso.ImageProcessing.Tests;

[TestFixture]
public class BlendTest {
    private readonly ClContext _clContext = new ClContext();
    private readonly Context _context = new();

    public BlendTest() {
        _context.Set(_clContext);
        _context.Set(new ImagePool(_context));
    }

    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        const Int32 w = 2000;
        const Int32 h = 2000;
        Vec4b[] pixels =
            Enumerable.Repeat(new Vec4b(1, 2, 3, 4), w * h)
                .ToArray();

        var a = new Image(_context, w, h, pixels, PixelFormat.Rgba8);
        var b = new Image(_context, w, h, pixels, PixelFormat.Rgba8);
        Image c;

        Blend blend = new Blend(_context);
        blend.Do(a, b, out c);

        var result = c.TakeCpuBuffer(Image.Operation.Read);
        var resultBytes = result.As<Vec4b>();


        Assert.Pass();
    }
}