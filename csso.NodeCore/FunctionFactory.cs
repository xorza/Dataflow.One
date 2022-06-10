using System;
using System.Collections.Generic;
using System.Linq;

namespace csso.NodeCore;

public class FunctionFactory {
    private readonly List<Function> _functions = new();

    public IReadOnlyList<Function> Functions => _functions.AsReadOnly();

    public Function Get(String functionName) {
        var result = _functions.SingleOrDefault(_ => _.Name == functionName);
        if (result == null) {
            throw new Exception("asfrt q34et43");
        }

        if (result is StatefulFunction factory) {
            return factory.CreateInstance();
        } else {
            return result;
        }
    }

    public Function Get(Guid functionId) {
        return _functions.Single(_ => _.Id == functionId);
    }

    public void Register(Function f) {
        if (_functions.Any(_ => _.Name == f.Name)) {
            throw new ArgumentException($"Function with name '{f.Name}' already registered");
        }

        if (_functions.Any(_ => _.Id != null && _.Id == f.Id)) {
            throw new ArgumentException($"Function with id '{f.Id}' already registered");
        }

        _functions.Add(f);
    }
}