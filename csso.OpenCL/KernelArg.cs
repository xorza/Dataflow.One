using csso.Common;

namespace csso.OpenCL; 

public enum Quantity {
    Scalar,
    Vector
}

public class KernelArg {
    public KernelArg(string name, string typeName) {
        Check.True(name.Length > 0);

        Name = name;

        var type = typeName;
        Quantity = Quantity.Scalar;

        if (typeName.EndsWith("*")) {
            Check.True(typeName.Length > 1);
            type = typeName.Substring(0, typeName.Length - 1);
            Quantity = Quantity.Vector;
        }

        Type = type.ToEnum<DataType>();
    }

    public string Name { get; }
    public Quantity Quantity { get; }
    public DataType Type { get; }
}