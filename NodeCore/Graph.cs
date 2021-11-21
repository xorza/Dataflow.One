using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csso.Common;

namespace csso.NodeCore
{
    public class Graph
    {
        private readonly List<Node> _nodes = new List<Node>();
        public IReadOnlyList<Node> Nodes { get; private set; }
        public List<Binding> Outputs { get; private set; } = new List<Binding>();

        public Graph()
        {
            this.Nodes = _nodes.AsReadOnly();
        }

        internal void Add(Node node)
        {
            Debug.Assert.AreSame(node.Graph, this);

            this._nodes.Add(node);
        }
    }
}
