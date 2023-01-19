using System.Linq;
using csso.Common;
using NUnit.Framework;

namespace csso.OpenCL.Tests;

public class ImageTests {
    private readonly ClContext _clContext = new();

    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        uint w = 7;
        uint h = 3;

        var image = new ClImage(_clContext, w, h, PixelFormat.Rgba8);
        var commandQueue = new ClCommandQueue(_clContext);

        var pixels = Enumerable
            .Repeat(new Vec4b(1, 2, 3, 4), (int) (w * h))
            .ToArray();

        image.Upload(commandQueue, pixels);

        pixels = Enumerable
            .Repeat(new Vec4b(0, 0, 0, 0), (int) (w * h))
            .ToArray();
        image.Download(commandQueue, pixels);

        Assert.Pass();
    }
}