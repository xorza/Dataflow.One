using System;
using System.IO;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.OpenCL;

namespace csso.ImageProcessing.Funcs;

public class FileImageSource : StatefulFunction , IDisposable{
    public FileInfo? FileInfo { get; set; }
    public ClBuffer? Buffer { get; set; }

    public ImageProcessingContext Context { get; }

    public FileImageSource(ImageProcessingContext ctx) {
        Context = ctx;
        Name = "Image from File";
        
        FileInfo = new FileInfo("D:\\2.jpg");
        
        Init(GetBuffer_Func);
    }

    private ClBuffer Load() {
        Image image = new Image(FileInfo!);
        var result = image.CreateBuffer(Context.ClContext);
        return result;
    }

    public override Function CreateInstance() {
        return new FileImageSource(Context);
    }

    private bool GetBuffer_Func([Output] out ClBuffer? buffer) {
        Buffer ??= Load();
        buffer = Buffer;

        return true;
    }

    public void Dispose() {
        Buffer?.Dispose();
    }
}