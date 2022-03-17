namespace csso.NodeCore;

public class GraphEvaluationException : Exception {
    public GraphEvaluationException(string message) : base(message) { }
}

public class ArgumentMissingException : GraphEvaluationException {
    public ArgumentMissingException(Node node, FunctionInput input)
        : base("Function input not provided.") {
        Node = node;
        Input = input;
    }

    public Node Node { get; }
    public FunctionInput Input { get; }
}