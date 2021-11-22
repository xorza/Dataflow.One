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
        public Binding[] Inputs { get; private set; }


        public Node(Schema schema, Graph graph)
        {
            Schema = schema;
            Graph = graph;

            Graph.Add(this);

            Inputs = new Binding[Schema.Inputs.Count];
            for (int i = 0; i < Schema.Inputs.Count; i++)
            {
                Inputs[i] = new EmptyBinding(this, Schema.Inputs[i]);
            }

            Graph = graph;
        }
    }
}
