using System.Collections.ObjectModel;

namespace csso.NodeCore;

public class FunctionFactory {
    private readonly Dictionary<String, Function> _functionsByName = new();
    private readonly Dictionary<Guid, Function> _functionsById = new();


    public FunctionFactory() { }


    public Function Get(String functionName) {
        if (_functionsByName.TryGetValue(functionName, out Function? func)) {
            return func;
        }

        throw new Exception("seruiyotg345");
    }
    public Function Get(Guid functionId) {
        if (_functionsById.TryGetValue(functionId, out Function? func)) {
            return func;
        }

        throw new Exception("e5ycv24");
    }

    public void Register(Function f) {
        _functionsByName.Add(f.FullName, f);
        if (f.Id != null) {
            _functionsById.Add(f.Id.Value, f);
        }
    }

    public Function[] BuildFunctionsArray() {
        return _functionsByName.Values
            .Concat(_functionsById.Values)
            .ToArray();
    }
}