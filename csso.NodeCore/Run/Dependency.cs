namespace csso.NodeCore.Run; 


public class Dependency {
    public Dependency(ExecutionNode executionNode, FunctionOutput output, ConnectionBehavior behavior) {
        Node = executionNode;
        Output = output;
        Behavior = behavior;
    }

    public ExecutionNode Node { get; }
    public FunctionOutput Output { get; }

    public ConnectionBehavior Behavior { get; }
}