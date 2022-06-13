using System;
using System.Linq;
using System.Text;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class ClContext : IDisposable {
    public ClContext() {
        IsDisposed = false;

        CL
            .GetPlatformIds(out var platformIds)
            .ValidateSuccess();

        foreach (var platform in platformIds)
            CL
                .GetPlatformInfo(platform, PlatformInfo.Name, out var val)
                .ValidateSuccess();

        foreach (var platform in platformIds) {
            CL
                .GetDeviceIds(platform, DeviceType.All, out var devices)
                .ValidateSuccess();

            var context = CL
                .CreateContext(IntPtr.Zero, devices, IntPtr.Zero,
                    IntPtr.Zero, out var result);
            result.ValidateSuccess();

            if (devices.Length == 0) continue;

            InternalCLContext = context;
            ClDevices = devices;
            SelectedClDevice = devices.First();
            return;
        }

        throw new InvalidOperationException("cannot create context");
    }

    internal CLContext InternalCLContext { get; }
    internal CLDevice[] ClDevices { get; }
    internal CLDevice SelectedClDevice { get; }

    public bool IsDisposed { get; private set; }

    public void Dispose() {
        IsDisposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }



    private void ReleaseUnmanagedResources() {
        CL.ReleaseContext(InternalCLContext);
    }

    internal void CheckIfDisposed() {
        if (IsDisposed) throw new InvalidOperationException("Already disposed.");
    }

    ~ClContext() {
        ReleaseUnmanagedResources();
    }
}