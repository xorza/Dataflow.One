using System.Collections.ObjectModel;

namespace csso.NodeCore;

public class FunctionFactory {
    private readonly Dictionary<String, Function> _functions = new();
    
    public ReadOnlyDictionary<String, Function> Functions { get; }

    public FunctionFactory() {
        Functions = new(_functions);
    }


    public Function Get(String fullName) {
        if (_functions.TryGetValue(fullName, out Function? func))
            return func;
        throw new Exception("seruiyotg345");
    }

    public void Register(Function f) {
        _functions.Add(f.FullName, f);
    }
}