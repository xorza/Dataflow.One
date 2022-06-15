using System;
using System.Linq;
using csso.Common;
using NUnit.Framework;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL.Tests;

public class ImageTests {
    private ClContext _clContext = new();

    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        UInt32 w = 32;
        UInt32 h = 32;

        var image = new ClImage(_clContext, w, h, PixelFormat.Rgba8);
        var commandQueue = new ClCommandQueue(_clContext);

        var pixels = Enumerable
            .Repeat(new Vec4b(1, 2, 3, 4), (Int32) (w * h))
            .ToArray();

        image.Upload(commandQueue, pixels);

        Assert.Pass();
    }
}