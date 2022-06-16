using System;
using System.Linq;
using csso.Common;
using NUnit.Framework;

namespace csso.OpenCL.Tests;

public class ImageTests {
    private ClContext _clContext = new();

    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        UInt32 w = 7;
        UInt32 h = 3;

        var image = new ClImage(_clContext, w, h, PixelFormat.Rgba8);
        var commandQueue = new ClCommandQueue(_clContext);

        var pixels = Enumerable
            .Repeat(new Vec4b(1, 2, 3, 4), (Int32) (w * h))
            .ToArray();

        image.Upload(commandQueue, pixels);

        pixels = Enumerable
            .Repeat(new Vec4b(0, 0, 0, 0), (Int32) (w * h))
            .ToArray();
        image.Download(commandQueue, pixels);

        Assert.Pass();
    }
}