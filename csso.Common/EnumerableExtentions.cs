using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.Common
{
    public static class EnumerableExtentions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e, Action<T> action) 
        {

            return e;
        }
    }
}
