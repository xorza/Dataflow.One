namespace csso.NodeCore;

public class GraphEvaluationException : Exception {
    public GraphEvaluationException(string message) : base(message) { }
}

public class InputMissingException : GraphEvaluationException {
    public Node Node { get; private set; }
    public FunctionInput Input { get; private set; }

    public InputMissingException(Node node, FunctionInput input) 
        : base("Function input not provided.") {
        Node = node;
        Input = input;
    }
}