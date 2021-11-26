using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
internal static partial class Xtensions {
    internal static String DecodeString(this byte[] bytes) {
        byte[] withoutNulls = bytes.Where(b => b != 0).ToArray();

        String result = System.Text.Encoding.Default.GetString(withoutNulls);
        result = result.Trim().Normalize();
        String.Intern(result);
        return result;
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    internal static void ValidateSuccess(this CLResultCode code) {
        if (code != CLResultCode.Success) {
            throw new OpenCLException(code);
        }
    }
    

}
}