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
    private readonly Context _context = new Context();
    public UiApi? UiApi { get; private set; }

    public ImageProcessingContext() {
        _context.Register(new ClContext());
    }

    public void Init(UiApi api) {
        UiApi = api;
    }

    public void RegisterFunctions(FunctionFactory functionFactory) {
        functionFactory.Register(new FileImageSource(_context));
        functionFactory.Register(new Function("Messagebox", Messagebox));
    }

    public void Dispose() {
        _context.Dispose();
    }

    [Description("messagebox")]
    [FunctionId("18D7EE8B-F4F6-4C72-932D-80A47AF12012")]
    private bool Messagebox(Object message) {
        Debug.WriteLine(message.ToString() + " we34v5y245");
        return true;
    }
}