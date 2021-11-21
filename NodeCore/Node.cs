using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class Node
    {
        public Schema Schema { get; private set; }
        public Graph Graph { get; private set; }
        public IReadOnlyList<Binding> Inputs { get; private set; }

        private readonly List<Binding> _inputs = new List<Binding>();

        public Node(Schema schema, Graph graph)
        {
            Inputs = _inputs.AsReadOnly();

            Schema = schema;
            Graph = graph;

            Graph.Add(this);

            foreach (var input in this.Schema.Inputs)
            {
                _inputs.Add(new EmptyBinding(this, input));
            }

            Graph = graph;
        }

        internal void Add(Binding binding)
        {
            _inputs.Add(binding);
        }
    }
}
