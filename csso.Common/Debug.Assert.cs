using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.Common
{
    public static partial class Debug
    {
        public static class Assert
        {
            [Conditional("DEBUG")]
            public static void True(bool condition)
            {
                if (!condition)
                    throw new AssertionException();
            }
            [Conditional("DEBUG")]
            public static void AreSame<T>(T a, T b) where T : class
            {
                if (!Object.ReferenceEquals(a, b))
                    throw new AssertionException();
            }
            [Conditional("DEBUG")]
            public static void NotNull<T>(T? o) where T : class
            {
                if (o == null)
                    throw new AssertionException();
            }
        }
    }
}
