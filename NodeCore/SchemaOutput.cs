using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class SchemaOutput : SchemaPut
    {
        public SchemaOutput()
        {
        }
        public SchemaOutput(String name, Type type) : base(name, type)
        {
        }
    }
}
