using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class OutputBinding : Binding
    {
        public Node SourceNode { get; private set; }
        public SchemaOutput Output { get; private set; }

        public OutputBinding(Node node,
            SchemaInput input,
            Node sourceNode,
            SchemaOutput output) :
            base(node, input)
        {
            SourceNode = sourceNode;
            Output = output;

            if (input.Type != output.Type)
            {
                throw new Exception("type mismatch");
            }
        }
    }
}
