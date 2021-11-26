using System;
using csso.Common;
using OpenTK.Graphics.ES11;

namespace csso.ImageProcessing {
public enum Quantity {
    Scalar,
    Vector
}

public class KernelArg {
    public String Name { get; }
    public Quantity Quantity { get; }
    public DataType Type { get; }

    public KernelArg(String name, String typeName) {
        Check.True(name.Length > 0);

        Name = name;

        String type = typeName;
        Quantity = Quantity.Scalar;

        if (typeName.EndsWith("*")) {
            Check.True(typeName.Length > 1);
            type = typeName.Substring(0, typeName.Length - 1);
            Quantity = Quantity.Vector;
        }

        Type = type.ToEnum<DataType>();
    }
}
}