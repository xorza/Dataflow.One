using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;

public class ExecutionGraph {
    public List<EvaluationNode> EvaluationNodes { get; private set; } = new ();
    public Graph Graph { get; }

    public ExecutionGraph(Graph graph) {
        Graph = graph;

        Sync();
    }

    public void Sync() {
        List<EvaluationNode> newEvaluationNodes = new(Graph.Nodes.Count);

        for (int i = 0; i < Graph.Nodes.Count; i++) {
            EvaluationNode? existing = EvaluationNodes.SingleOrDefault(_ => _.Node == Graph.Nodes[i]);

            if (existing != null) {
                newEvaluationNodes.Add(existing);
            } else {
                newEvaluationNodes.Add(new EvaluationNode(Graph.Nodes[i]));
            }
        }

        EvaluationNodes = newEvaluationNodes;

        ValidateNodeOrder();
    }
    
    [Conditional("DEBUG")]
    private void ValidateNodeOrder() {
        Debug.Assert.True(EvaluationNodes.Count == Graph.Nodes.Count);

        for (int i = 0; i < EvaluationNodes.Count; i++) {
            Debug.Assert.AreSame(EvaluationNodes[i].Node, Graph.Nodes[i]);
        }
    }
}