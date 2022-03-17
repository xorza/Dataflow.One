namespace csso.NodeCore;

public class WithId {
    protected WithId() { }

    protected WithId(Guid id) {
        Id = id;
    }

    public Guid Id { get; protected set; } = Guid.NewGuid();
}