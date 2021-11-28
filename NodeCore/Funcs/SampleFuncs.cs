using System;
using System.ComponentModel;
using System.Runtime.InteropServices;


namespace csso.NodeCore.Funcs {
public static class F {
    public static bool Add(Int32 a, Int32 b, [Out] out Int32 result) {
        result = a + b;
        return true;
    }

    
        
    [Description("testestsetsetset")]
    public static bool DivideWhole(
        Int32 a,
        Int32 b,
        [Description("testestsetsetset1")]
        [Out] out Int32 result,
        [Out] out Int32 remainder
        ) {
        result = a / b;
        remainder = a % b;
        return true;
    }
}
}