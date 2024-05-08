using System.Linq;
using csso.Common;
using csso.ImageProcessing.Funcs;
using csso.OpenCL;
using NUnit.Framework;

namespace csso.ImageProcessing.Tests;

[TestFixture]
public class BlendTest {
    [SetUp]
    public void Setup() {
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _clContext.Dispose();
        _context.Dispose();
    }

    private readonly ClContext _clContext = new();
    private readonly Context _context = new();

    public BlendTest() {
        _context.Set(_clContext);
        _context.Set(new ImagePool(_context));
    }

    [Test]
    public void RgbaTest() {
        const int width = 3;
        const int height = 2;
        const PixelFormat pixelFormat = PixelFormat.Rgba8;

        var pixels =
            Enumerable
                .Repeat(new Vec4b(255, 128, 64, 32), width * height)
                .ToArray();

        using var a = new Image(_context, pixelFormat, width, height);
        using var b = new Image(_context, pixelFormat, width, height);
        a.Set(pixels);
        b.Set(pixels);

        var blend = new Blend(_context);

        blend.Do(a, b, out var c);

        using (c) {
            var buffer = c.TakeCpuBuffer(Image.Operation.Read);

            for (uint w = 0; w < width; w++)
            for (uint h = 0; h < height; h++) {
                var v = c.Get<Vec4b>(w, h);

                Assert.That(v, Is.EqualTo(new Vec4b(255, 64, 16, 4)));
            }
        }

        Assert.Pass();
    }
}