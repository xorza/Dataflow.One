using System;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL;

public class OpenCLException : Exception {
    internal OpenCLException(CLResultCode clResultCode) {
        ClResultCode = clResultCode;
    }

    internal CLResultCode ClResultCode { get; }
}