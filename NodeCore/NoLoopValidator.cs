using csso.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class NoLoopValidator
    {
        private struct Data
        {
            public bool visited = false;
        }

        public void Go(Graph graph)
        {
            Int32 nodeCount = graph.Nodes.Count;

            List<Node> path = new List<Node>();
            foreach (Binding binding in graph.Outputs)
            {
                Go(binding, path);
            }
        }

        private void Go(Binding binding, List<Node> pathBack)
        {
            OutputBinding? outputBinding = binding as OutputBinding;
            if (outputBinding == null)
            {
                return;
            }
            Node node = outputBinding.SourceNode;

            if (pathBack.Contains(node))
            {
                throw new Exception("loop detected");
            }

            pathBack.Add(node);

            for (int i = 0; i < node.Inputs.Count; i++)
            {
                Go(node.Inputs[i], pathBack);
            }

            pathBack.RemoveAt(pathBack.Count - 1);
        }

    }
}
