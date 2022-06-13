using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using csso.ImageProcessing.Funcs;
using csso.NodeCore;
using csso.NodeRunner.Shared;
using csso.OpenCL;

namespace csso.ImageProcessing;

public class ImageProcessingContext : IComputationContext, IDisposable {
    private readonly Context _context = new();

    private readonly ClContext _clContext = new();
    private readonly ImagePool _imagePool;

    public UiApi? UiApi { get; private set; }

    public ImageProcessingContext() {
        _imagePool = new(_context);
    }

    public void Init(UiApi api) {
        UiApi = api;
    }

    public void RegisterFunctions(FunctionFactory functionFactory) {
        functionFactory.Register(new FileImageSource(_context));
        functionFactory.Register(new Blend(_context));
        

        functionFactory.Register(new Function("Messagebox", Messagebox));
    }

    public void OnStartRun() {
        _context.Set(_clContext);
        _context.Set(_imagePool);
    }

    public void OnFinishRun() {
        _context.Remove(_clContext);
        _context.Remove(_imagePool);
    }

    public void Dispose() {
        _context.Dispose();
    }

    [Description("messagebox")]
    [FunctionId("18D7EE8B-F4F6-4C72-932D-80A47AF12012")]
    private bool Messagebox(Image img) {
        var bmpSource = img.ConvertToBitmapSource();
        UiApi!.ShowImage(bmpSource);

        return true;
    }
}