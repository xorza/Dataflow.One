namespace dfo.NodeCore;

public class Event {
    public Event(string name) {
        Name = name;
    }

    public string Name { get; }
    public bool IsActive { get; } = false;
}

public class AlwaysEvent : Event {
    public AlwaysEvent() : base("Always") { }
}

public class EventSubscription {
    public EventSubscription(Event @event, Node node) {
        Event = @event;
        Node = node;
    }

    public Event Event { get; }
    public Node Node { get; }
}