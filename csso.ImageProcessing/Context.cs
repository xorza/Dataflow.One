using System;
using System.Collections.Generic;
using System.Linq;
using csso.Common;

namespace csso.ImageProcessing;

public class Context:IDisposable {
    private readonly Dictionary<Type, Object> _services = new();

    public Context() { }

    public void Register<T>(T service) {
        Check.Argument(service != null, nameof(service));

        Type type = typeof(T);
        _services.Add(type, service!);
    }

    public T Resolve<T>() {
        Type type = typeof(T);
        T service = (T)_services[type];
        return service;
    }

    public void Dispose() {
        _services.Values
            .OfType<IDisposable>()
            .ForEach(_ => _.Dispose());
    }
}