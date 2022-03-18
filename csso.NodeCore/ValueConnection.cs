using csso.Common;

namespace csso.NodeCore;

public class ValueConnection : Connection {
    public ValueConnection(Node node, FunctionInput input) : base(node, input) { }

    public ValueConnection(Node node, SerializedValueConnection serialized) {
        Node = node;
        Input = Node.Inputs
            .Single(input => input.ArgumentIndex == serialized.InputIndex);

        if (
            serialized.Value != null
            && StringParser.TryParse(serialized.Value, Input.Type, out var value))
            Value = value;
    }

    public Object? Value { get; set; }

    public SerializedValueConnection Serialize() {
        SerializedValueConnection result = new();

        result.InputIndex = Input.ArgumentIndex;
        result.InputNodeId = Node.Id;
        result.Value = Value?.ToString();

        return result;
    }
}

public class SerializedValueConnection {
    public Int32 InputIndex { get; set; }
    public Guid InputNodeId { get; set; }
    public String? Value { get; set; }
}