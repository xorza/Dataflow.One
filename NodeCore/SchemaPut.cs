using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class SchemaPut
    {
        public Type Type { get; set; } = typeof(void);
        public String Name { get; set; } = "";

        public SchemaPut()
        {
        }
        public SchemaPut(String name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
