﻿using System;

namespace dfo.NodeCore;

public class GraphEvaluationException : Exception {
    public GraphEvaluationException(string message) : base(message) { }
}

public class ArgumentMissingException : GraphEvaluationException {
    public ArgumentMissingException(Node node, FunctionArg input)
        : base("Function input not provided.") {
        Node = node;
        Input = input;
    }

    public Node Node { get; }
    public FunctionArg Input { get; }
}