using System;
using System.IO;
using csso.NodeCore;

namespace csso.ImageProcessing.Funcs;

public class FileImageSource : StatefulFunction, IDisposable {
    private readonly Context _context;

    public FileImageSource(Context ctx) {
        _context = ctx;
        Name = "Image from File";

        FileInfo = new FileInfo("D:\\2.jpg");

        SetFunction(GetBuffer_Func);
    }

    public FileInfo? FileInfo { get; }
    public Image? Image { get; set; }

    public void Dispose() {
        Image?.Dispose();
    }

    private Image Load() {
        var image = new Image(_context, FileInfo!);
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
}