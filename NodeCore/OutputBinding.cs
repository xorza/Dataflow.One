using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class OutputBinding : Binding
    {
        public Node OutputNode { get; private set; }
        public SchemaOutput Output { get; private set; }

        public OutputBinding(Node inputNode,
            SchemaInput input,
            Node outputNode,
            SchemaOutput output) :
            base(inputNode, input)
        {
            OutputNode = outputNode;
            Output = output;

            if (input.Type != output.Type)
            {
                throw new Exception("type mismatch");
            }
        }
    }
}
