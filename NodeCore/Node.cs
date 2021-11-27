using System.Collections.Generic;
using csso.Common;

namespace csso.NodeCore {
public class Node {
    private readonly List<Binding> _inputBindings = new();

    public Node(Schema schema, Graph graph) {
        Schema = schema;
        Graph = graph;

        Graph.Add(this);

        Inputs = _inputBindings.AsReadOnly();
    }

    public Schema Schema { get; }
    public Graph Graph { get; }
    public IReadOnlyList<Binding> Inputs { get; }

    public void AddBinding(Binding binding) {
        _inputBindings.RemoveAll(_ => _.Input == binding.Input);

        Check.True(binding.InputNode == this);

        _inputBindings.Add(binding);
    }
}
}