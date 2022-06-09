namespace csso.NodeCore;

public class FunctionFactory {
    private readonly List<Function> _functions = new();

    public IReadOnlyList<Function> Functions => _functions.AsReadOnly();
    
    public Function Get(String functionName) {
       var result = _functions.SingleOrDefault(_ => _.Name == functionName);
       if (result != null) {
           return result;
       } else {
           throw new Exception("seruiyotg345");
       }
    }

    public Function Get(Guid functionId) {
    
        var result = _functions.SingleOrDefault(_ => _.Id == functionId);

        if (result != null) {
            return result;
        } else {
          
            throw new Exception("e5ycv24");
        }
    }

    public void Register(Function f) {
        _functions.Add(f);
    }
}
