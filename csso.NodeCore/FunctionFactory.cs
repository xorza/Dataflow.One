namespace csso.NodeCore;

public class FunctionFactory {
    private readonly Dictionary<String, Function> _functions = new();


    public FunctionFactory() { }


    public Function Get(String fullName) {
        if (_functions.TryGetValue(fullName, out Function? func))
            return func;
        throw new Exception("seruiyotg345");
    }

    public void Register(Function f) {
        _functions.Add(f.FullName, f);
    }
}