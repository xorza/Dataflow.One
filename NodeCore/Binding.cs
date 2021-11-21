﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class Binding
    {
        public SchemaInput Input { get; private set; }
        public Node Node { get; private set; }

        public Binding(Node node, SchemaInput input)
        {
            Input = input;
            Node = node;

            Node.Add(this);
        }
    }
}
