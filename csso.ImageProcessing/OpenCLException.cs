using System;
using OpenTK.Compute.OpenCL;

namespace csso.ImageProcessing {
public class OpenCLException :Exception{
    internal CLResultCode ClResultCode { get; }

    internal OpenCLException(CLResultCode clResultCode) {
        ClResultCode = clResultCode;
    }
}
}