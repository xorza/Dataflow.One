namespace csso.NodeCore;

public class FunctionIdAttribute : Attribute {
    public Guid Id { get; set; }

    public FunctionIdAttribute(Guid id) {
        Id = id;
    }

    public FunctionIdAttribute(string guid) {
        Id = Guid.Parse(guid);
    }
}