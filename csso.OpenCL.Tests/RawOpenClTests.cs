using System;
using System.Linq;
using csso.Common;
using NUnit.Framework;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL.Tests;

public class RawOpenClTests {
    private CLContext _clContext;
    private CLDevice _device;


    [SetUp]
    public void Setup() {
        CL
            .GetPlatformIds(out var platformIds);

        foreach (var platform in platformIds) {
            CL
                .GetDeviceIds(platform, DeviceType.Gpu, out var devices);

            if (devices.Length == 0) continue;

            var context = CL
                .CreateContext(IntPtr.Zero, devices, IntPtr.Zero,
                    IntPtr.Zero, out var result);

            _clContext = context;
            _device = devices.First();
            return;
        }
    }

    [Test]
    public void Test1() {
        const UInt32 memoryAlignment = 64;

        const UInt32 width = 32;
        const UInt32 height = 32;
        const UInt32 stride = (width * 4 + (memoryAlignment - 1)) & ~(memoryAlignment - 1);
        const UInt32 sizeInBytes = stride * height;

        var imageFormat = new ImageFormat() {
            ChannelOrder = ChannelOrder.Rgba,
            ChannelType = ChannelType.UnsignedInteger8
        };
        var imageDescription = ImageDescription.Create2D(
            width,
            height,
            stride
        );

        var image = CL.CreateImage(
            _clContext,
            MemoryFlags.ReadWrite,
            ref imageFormat,
            ref imageDescription,
            IntPtr.Zero,
            out var result);
        Assert.That(result, Is.EqualTo(CLResultCode.Success));

        var commandQueue = CL.CreateCommandQueueWithProperties(
            _clContext,
            _device,
            IntPtr.Zero,
            out result
        );
        Assert.That(result, Is.EqualTo(CLResultCode.Success));

        var data = Memory.Alloc(sizeInBytes);

        result = CL.EnqueueWriteImage(
            commandQueue,
            image,
            true,
            new UIntPtr[3] {UIntPtr.Zero, UIntPtr.Zero, UIntPtr.Zero},
            new UIntPtr[3] {new(width), new(height), new(1)},
            new UIntPtr(stride),
            UIntPtr.Zero,
            data,
            0,
            null,
            out var clEvent
        );
        Assert.That(result, Is.EqualTo(CLResultCode.Success));

        Assert.Pass();
    }
}