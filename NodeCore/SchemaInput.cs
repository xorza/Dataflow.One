using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class SchemaInput : SchemaPut
    {
        public SchemaInput()
        {
        }
        public SchemaInput(String name, Type type) : base(name, type)
        {
        }
    }
}
