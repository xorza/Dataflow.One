using System.Collections.ObjectModel;
using csso.Common;
using csso.NodeCore;

namespace csso.NodeRunner.UI;

public class FunctionFactoryView {
    private readonly ObservableCollection<Function> _functions = new();

    public FunctionFactoryView() {
        Functions = new ReadOnlyObservableCollection<Function>(_functions);
    }

    public ReadOnlyObservableCollection<Function> Functions { get; }

    public void Sync(FunctionFactory functionFactory) {
        _functions.Clear();
        functionFactory.Functions
            .AddTo(_functions);
    }
}