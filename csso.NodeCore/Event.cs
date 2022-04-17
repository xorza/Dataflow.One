﻿namespace csso.NodeCore;

public class Event {
    public string Name { get; private set; }
    public bool IsActive { get; private set; } = false;

    public Node Owner { get; internal set; }

    public Event(string name) {
        Name = name;
    }
}

public class AlwaysEvent : Event {
    public AlwaysEvent() : base("Always") { }
}

public class Subscription {
    public Event Event { get; private set; }
    public Node Node { get; private set; }
    
    
    public Subscription(Event @event, Node node) {
        Event = @event;
        Node = node;
    }
    
}