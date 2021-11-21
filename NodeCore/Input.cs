using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class Input
    {
        public Node Node { get; private set; }

        public Input(Node node) => this.Node = node;
    }
}
