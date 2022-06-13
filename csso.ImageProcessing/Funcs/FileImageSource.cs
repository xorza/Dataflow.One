using System;
using System.IO;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.OpenCL;

namespace csso.ImageProcessing.Funcs;

public class FileImageSource : StatefulFunction, IDisposable {
    public FileInfo? FileInfo { get;  }
    public Image? Image { get; set; }

    private readonly Context _context;

    public FileImageSource(Context ctx) {
        _context = ctx;
        Name = "Image from File";
        
        FileInfo = new FileInfo("D:\\2.jpg");
        
        SetFunction(GetBuffer_Func);
    }

    private Image Load() {
        Image image = new Image(_context, FileInfo!);
        return image;
    }

    public override Function CreateInstance() {
        return new FileImageSource(_context);
    }

    private bool GetBuffer_Func([Output] out Image? image) {
        Image ??= Load();
        image = Image;

        return true;
    }

    public void Dispose() {
        Image?.Dispose();
    }
}