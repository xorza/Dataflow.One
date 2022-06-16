using System;
using System.Linq;
using csso.Common;
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

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _clContext.Dispose();
        _context.Dispose();
    }

    [Test]
    public void Test1() {
        const Int32 width = 3;
        const Int32 height = 2;
        const PixelFormat pixelFormat = PixelFormat.Rgba8;

        Vec4b[] pixels =
            Enumerable
                .Repeat(new Vec4b(255, 128, 64, 32), width * height)
                .ToArray();

        using var a = new Image(_context, pixelFormat, width, height);
        using var b = new Image(_context, pixelFormat, width, height);
        a.Set(pixels);
        b.Set(pixels);

        Blend blend = new Blend(_context);

        blend.Do(a, b, out var c);
        Assert.NotNull(c);

        using (c) {
            var buffer = c.TakeCpuBuffer(Image.Operation.Read);
         
            for (UInt32 w = 0; w < width; w++) {
                for (UInt32 h = 0; h < height; h++) {
                    Vec4b v = c.Get<Vec4b>(w, h);
            
                    Assert.That(v, Is.EqualTo(new Vec4b(255, 64, 16, 4)));
                }
            }
        }

        Assert.Pass();
    }
}