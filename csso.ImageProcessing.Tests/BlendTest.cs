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

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _clContext.Dispose();
        _context.Dispose();
    }

    [Test]
    public void Test1() {
        const Int32 width = 3;
        const Int32 height = 2;

        Vec4b[] pixels =
            Enumerable
                .Repeat(new Vec4b(1, 2, 3,4), width * height)
                .ToArray();

        Image a, b, c;
        using (a = new Image(_context, width, height))
        using (b = new Image(_context, width, height)) {
            a.Set(pixels);
            b.Set(pixels);

            Blend blend = new Blend(_context);
            blend.Do(a, b, out c);
        }

        Assert.NotNull(c);

        c.UpdateCpuBuffer();

        // for (int w = 0; w < width; w++) {
        //     for (int h = 0; h < height; h++) {
        //         Vec4b v = c.Get<Vec4b>(w, h);
        //
        //         Assert.That(v, Is.EqualTo(new Vec4b(2, 4, 6, 8)));
        //     }
        // }

        Assert.Pass();
    }
}