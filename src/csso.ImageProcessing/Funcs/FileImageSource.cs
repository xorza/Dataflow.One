using System;
using System.IO;
using csso.NodeCore;

namespace csso.ImageProcessing.Funcs;

public class FileImageSource : StatefulFunction, IDisposable {
    private readonly Context _context;

    private readonly FileInfo? _fileInfo = null;

    public FileImageSource(Context ctx) {
        _context = ctx;
        Name = "Image from File";

        FileInfo = new FileInfo("D:\\2.png");

        SetFunction(GetBuffer_Func);

        Behavior = FunctionBehavior.Reactive;
    }

    public FileInfo? FileInfo {
        get => _fileInfo;
        set {
            if (_fileInfo == value) {
                return;
            }

            Image?.Dispose();
        }
    }

    public Image? Image { get; private set; }

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