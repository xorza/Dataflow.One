using System;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
public class OpenCLException :Exception{
    internal CLResultCode ClResultCode { get; }

    internal OpenCLException(CLResultCode clResultCode) {
        ClResultCode = clResultCode;
    }
}
}