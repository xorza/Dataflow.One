using System.Collections.ObjectModel;
using System.Linq;
using csso.Common;
using csso.NodeCore;
using DynamicData;

namespace csso.WpfNode;

public class FunctionFactoryView {
    private readonly ObservableCollection<Function> _functions = new();

    public FunctionFactoryView(FunctionFactory functionFactory) {
        FunctionFactory = functionFactory;
        Functions = new ReadOnlyObservableCollection<Function>(_functions);

        _functions.AddRange(FunctionFactory.Functions);
    }

    public FunctionFactory FunctionFactory { get; }
    public ReadOnlyObservableCollection<Function> Functions { get; }
}