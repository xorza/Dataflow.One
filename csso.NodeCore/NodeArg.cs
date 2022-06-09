namespace csso.NodeCore; 

public class NodeArg {
    public FunctionArg FunctionArg { get; internal set; }
    public Node Node { get; internal set; }
    public ArgDirection ArgDirection => FunctionArg.ArgDirection;
    public Type Type => FunctionArg.Type;
}