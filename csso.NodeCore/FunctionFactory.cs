namespace csso.NodeCore;

public class FunctionFactory {
    private readonly Dictionary<Guid, Function> _functions = new();


    public FunctionFactory() { }


    public Function Get(Guid id) {
        if (_functions.TryGetValue(id, out Function? func))
            return func;
        throw new Exception("seruiyotg345");
    }

    public void Register(Function f) {
        _functions.Add(f.Id, f);
    }
}