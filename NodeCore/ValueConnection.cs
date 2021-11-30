using System;

namespace csso.NodeCore {
public class ValueConnection : Connection {
    public ValueConnection(Node node, FunctionInput input) : base(node, input) { }

    public Object? Value { get; set; }


    public override FunctionBehavior FinalBehavior {
        get => FunctionBehavior.Reactive;
    }
}
}