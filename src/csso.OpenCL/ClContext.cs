using System;
using System.Linq;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class ClContext : IDisposable {
    public ClContext() {
        IsDisposed = false;

        CL
            .GetPlatformIds(out var platformIds)
            .ValidateSuccess();

        // foreach (var platform in platformIds)
        //     CL
        //         .GetPlatformInfo(platform, PlatformInfo.Name, out var val)
        //         .ValidateSuccess();

        foreach (var platform in platformIds) {
            CL
                .GetDeviceIds(platform, DeviceType.Gpu, out var devices)
                .ValidateSuccess();

            if (devices.Length == 0) {
                throw new OpenCLException(CLResultCode.DeviceNotFound);
            }

            var context = CL
                .CreateContext(
                    IntPtr.Zero,
                    devices,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out var result);
            result.ValidateSuccess();

            RawClContext = context;
            ClDevices = devices;
            SelectedClDevice = devices.First();
            return;
        }

        throw new InvalidOperationException("cannot create context");
    }

    internal CLContext RawClContext { get; }
    internal CLDevice[] ClDevices { get; }
    internal CLDevice SelectedClDevice { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    private void ReleaseUnmanagedResources() {
        CL.ReleaseContext(RawClContext);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed) {
            throw new InvalidOperationException("Already disposed.");
        }
    }

    ~ClContext() {
        ReleaseUnmanagedResources();
    }
}