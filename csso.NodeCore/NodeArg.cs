namespace csso.NodeCore; 

public class NodeArg {
    public FunctionArg FunctionArg { get; internal set; }
    public Node Node { get; internal set; }
    public ArgType ArgType => FunctionArg.ArgType;
    public Type Type => FunctionArg.Type;
}