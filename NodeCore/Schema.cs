using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.NodeCore
{
    public class Schema
    {
        public List<SchemaInput> Inputs { get;private set; } = new List<SchemaInput>();
        public List<SchemaOutput> Outputs { get; private set; } = new List<SchemaOutput>();
        public String Name { get; set; } = "";


        public Schema()
        {
        }
    }
}
