namespace csso.NodeCore;

public class WithId {
    public Guid Id { get; protected set; } = Guid.NewGuid();
    protected WithId() { }

    protected WithId(Guid id) {
        Id = id;
    }
}