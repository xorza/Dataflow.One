namespace csso.NodeCore;

public class NoLoopValidator {
    public void Go(Graph graph) {
        var nodeCount = graph.Nodes.Count;

        List<Node> path = new();
        foreach (var outputNode in graph.Nodes) {
            foreach (var binding in graph.GetDataSubscriptions(outputNode)) {
                Go(binding, path);
            }
        }
    }

    private void Go(DataSubscription dataSubscription, List<Node> pathBack) {
        var node = dataSubscription.TargetNode;

        if (pathBack.Contains(node)) {
            throw new Exception("loop detected");
        }

        pathBack.Add(node);

        foreach (var b in node.Graph.GetDataSubscriptions(node)) {
            Go(b, pathBack);
        }

        pathBack.RemoveAt(pathBack.Count - 1);
    }
}