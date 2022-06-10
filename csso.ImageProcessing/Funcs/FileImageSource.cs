using csso.NodeCore.Funcs;
using csso.OpenCL;

namespace csso.ImageProcessing.Funcs;

public class FileImageSource : ConstantFunc<Buffer> {
    public FileImageSource() : base("Image from file", null) { }
}