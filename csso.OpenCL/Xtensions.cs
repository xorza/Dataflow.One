using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Compute.OpenCL;

namespace csso.OpenCL {
internal static partial class Xtensions {
    internal static string DecodeString(this byte[] bytes) {
        byte[] withoutNulls = bytes.Where(b => b != 0).ToArray();

        string result = Encoding.Default.GetString(withoutNulls);
        result = result.Trim().Normalize();
        string.Intern(result);
        return result;
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    internal static void ValidateSuccess(this CLResultCode code) {
        if (code != CLResultCode.Success) throw new OpenCLException(code);
    }
}
}