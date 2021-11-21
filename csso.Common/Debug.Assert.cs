using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.Common
{
    public static partial class Debug
    {
        public static class Assert
        {
            public static void True(bool condition)
            {
                if (!condition)
                    throw new AssertionException();
            }
            public static void AreSame<T>(T a, T b) where T : class
            {
                if (!Object.ReferenceEquals(a, b))
                    throw new AssertionException();
            }
        }
    }
}
