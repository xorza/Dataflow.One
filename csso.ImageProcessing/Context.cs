using System;
using System.Collections.Generic;
using System.Linq;
using csso.Common;

namespace csso.ImageProcessing;

public class Context : IDisposable {
    private readonly Dictionary<Type, Object> _services = new();

    public Context() { }

    public void Set<T>(T service) where T : class {
        Type type = typeof(T);
        _services.Add(type, service!);
    }

    public T Get<T>() where T : class {
        Type type = typeof(T);
        T service = (T) _services[type];
        return service;
    }

    public void Remove<T>() where T : class {
        Type type = typeof(T);
        _services.Remove(type);
    }

    public void Remove<T>(T service) where T : class {
        _services
            .Where(kvp => kvp.Value == service)
            .ToList()
            .ForEach(_ => _services.Remove(_.Key));
    }


    public void Dispose() {
        _services.Values
            .OfType<IDisposable>()
            .ForEach(_ => _.Dispose());
    }
}