using System;

namespace dfo.NodeCore;

public class FunctionIdAttribute : Attribute {
    public FunctionIdAttribute(Guid id) {
        Id = id;
    }

    public FunctionIdAttribute(string guid) {
        Id = Guid.Parse(guid);
    }

    public Guid Id { get; set; }
}