using System.Collections.ObjectModel;
using System.Linq;
using csso.Common;
using csso.NodeCore;

namespace csso.WpfNode;

public class FunctionFactoryView {
    public FunctionFactory FunctionFactory { get; }

    private readonly ObservableCollection<Function> _functions = new();
    public ReadOnlyObservableCollection<Function> Functions { get; }

    public FunctionFactoryView(FunctionFactory functionFactory) {
        FunctionFactory = functionFactory;
        Functions = new(_functions);

        FunctionFactory.BuildFunctionsArray()
            .Foreach(_functions.Add);
        
        
    }
}
