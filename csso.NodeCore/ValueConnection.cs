using csso.Common;

namespace csso.NodeCore;

public class ValueConnection : Connection {
    public ValueConnection(Node node, FunctionInput input) : base(node, input) { }

    public Object? Value { get; set; }

    public SerializedValueConnection Serialize() {
        SerializedValueConnection result = new();

        result.InputIndex = Input.Index;
        result.InputNodeId = Node.Id;
        result.Value = Value?.ToString();

        return result;
    }

    public ValueConnection(Node node, SerializedValueConnection serialized) {
        Node = node;
        Input = Node.Function.Inputs
            .Single(input => input.Index == serialized.InputIndex);

        if (
            serialized.Value != null
            && StringParser.TryParse(serialized.Value, Input.Type, out Object? value))
            Value = value;
    }
}

public class SerializedValueConnection {
    public Int32 InputIndex { get; set; }
    public Guid InputNodeId { get; set; }
    public String? Value { get; set; }
}