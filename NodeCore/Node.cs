using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using csso.Common;

namespace csso.NodeCore {
public class Node {
    private readonly List<Connection> _connections = new();


    public Node(IFunction function, Graph graph) {
        Inputs = function.Inputs;
        Outputs = function.Outputs;
        Function = function;
        Graph = graph;

        Connections = _connections.AsReadOnly();
    }


    public IReadOnlyList<FunctionInput> Inputs { get; }
    public IReadOnlyList<FunctionOutput> Outputs { get; }

    public string Name => Function.Name;

    public IFunction Function { get; private set; }
    public Graph Graph { get; }
    public IReadOnlyList<Connection> Connections { get; }

    public void AddBinding(Connection connection) {
        _connections.RemoveAll(_ => _.Input == connection.Input);

        Check.True(connection.InputNode == this);

        _connections.Add(connection);
    }
}
}