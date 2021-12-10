namespace csso.NodeCore; 

public class WithId {
    public Guid Id { get; set; } = Guid.NewGuid();
    public WithId() { }
    public WithId(Guid id) {
        Id = id;
    }
}