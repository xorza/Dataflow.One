using System.Collections.ObjectModel;
using csso.NodeCore;
using DynamicData;

namespace csso.NodeRunner.UI;

public class FunctionFactoryView {
    private readonly ObservableCollection<Function> _functions = new();

    public FunctionFactoryView() {
        Functions = new ReadOnlyObservableCollection<Function>(_functions);
    }

    public ReadOnlyObservableCollection<Function> Functions { get; }
    
    public  void Sync(FunctionFactory functionFactory) {
        _functions.Clear();
        _functions.AddRange(functionFactory.Functions);
    }
}