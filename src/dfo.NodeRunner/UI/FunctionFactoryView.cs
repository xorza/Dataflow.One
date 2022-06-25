using System.Collections.ObjectModel;
using dfo.Common;
using dfo.NodeCore;
using dfo.NodeRunner.Infrastructure;

namespace dfo.NodeRunner.UI;

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