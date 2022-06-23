using System;
using System.Collections.Generic;
using System.Linq;
using dfo.Common;

namespace dfo.ImageProcessing;

public class Context : IDisposable {
    private readonly Dictionary<Type, object> _services = new();


    public void Dispose() {
        _services.Values
            .OfType<IDisposable>()
            .ForEach(_ => _.Dispose());
    }

    public void Set<T>(T service) where T : class {
        var type = typeof(T);
        _services.Add(type, service!);
    }

    public T Get<T>() where T : class {
        var type = typeof(T);
        var service = (T) _services[type];
        return service;
    }

    public void Remove<T>() where T : class {
        var type = typeof(T);
        _services.Remove(type);
    }

    public void Remove<T>(T service) where T : class {
        _services
            .Where(kvp => kvp.Value == service)
            .ToList()
            .ForEach(_ => _services.Remove(_.Key));
    }
}