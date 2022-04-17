using System.Collections.ObjectModel;
using csso.Common;
using csso.NodeCore;

namespace csso.WpfNode;

public class FunctionFactoryView {
    private readonly ObservableCollection<Function> _functions = new();

    public FunctionFactoryView(FunctionFactory functionFactory) {
        FunctionFactory = functionFactory;
        Functions = new ReadOnlyObservableCollection<Function>(_functions);

        FunctionFactory.BuildFunctionsArray()
            .ForEach(_functions.Add);
    }

    public FunctionFactory FunctionFactory { get; }
    public ReadOnlyObservableCollection<Function> Functions { get; }
}