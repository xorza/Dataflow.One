using System;
using OpenTK.Compute.OpenCL;

namespace dfo.OpenCL;

internal class OpenCLException : Exception {
    internal OpenCLException(CLResultCode clResultCode)
        : base(clResultCode.ToString()) {
        ClResultCode = clResultCode;
    }

    public CLResultCode ClResultCode { get; }

    public override string ToString() {
        return ClResultCode.ToString();
    }
}