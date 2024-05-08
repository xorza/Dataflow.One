using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore;
using csso.NodeCore.Annotations;
using csso.NodeCore.Funcs;

namespace csso.Nodeshop.UI;

public abstract class EditableValueView : INotifyPropertyChanged {
    protected EditableValueView(Type type) {
        Type = type;
    }

    public Type Type { get; }

    public virtual bool HasValue => true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static EditableValueView Create(PutView inputView) {
        var editableValueView =
            typeof(InputArgValueView<>)
                .MakeGenericType(inputView.NodeArg.Type)
                .GetConstructors()
                .First()
                .Invoke(new object[] { inputView });
        return (EditableValueView)editableValueView;
    }

    public static EditableValueView Create(ConstantFunc func) {
        var editableValueView =
            typeof(ConstantValueFuncView<>)
                .MakeGenericType(func.Type)
                .GetConstructors()
                .First()
                .Invoke(new object[] { func });
        return (EditableValueView)editableValueView;
    }

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected abstract void ResetValue();
}

public class InputArgValueView<T> : EditableValueView {
    public InputArgValueView(PutView inputView) : base(inputView.NodeArg.Type) {
        Check.Argument(inputView.ArgDirection == ArgDirection.In, nameof(inputView));

        InputView = inputView;
    }

    public PutView InputView { get; }

    public DataSubscription? DataSubscription {
        get => InputView.GetDataSubscription();
        set {
            if (value == null) {
                InputView.NodeView.Node.Graph.RemoveSubscription(InputView.NodeArg);
            } else {
                InputView.NodeView.Node.Graph.Add(value);
            }

            InputView.NodeView.GraphView.Sync();
        }
    }

    public T? Value {
        get {
            if (DataSubscription?.Value == null) {
                return default;
            }

            return (T?)DataSubscription.Value;
        }
        set {
            if (Equals(value, DataSubscription?.Value)) {
                return;
            }

            if (value == null) {
                ResetValue();
            } else {
                DataSubscription = new DataSubscription(InputView.NodeArg, value);
            }

            OnPropertyChanged();
        }
    }

    public override bool HasValue => DataSubscription?.Value != null;

    protected override void ResetValue() {
        DataSubscription = null;
    }
}

public class ConstantValueFuncView<T> : EditableValueView {
    private readonly ConstantFunc<T> _constantFunc;

    public ConstantValueFuncView(ConstantFunc<T> constantFunc) : base(typeof(T)) {
        _constantFunc = constantFunc;
    }

    public T? Value {
        get => _constantFunc.TypedValue;
        set {
            if (EqualityComparer<T>.Default.Equals(value, _constantFunc.TypedValue)) {
                return;
            }

            _constantFunc.TypedValue = value;
            OnPropertyChanged();
        }
    }

    protected override void ResetValue() {
        _constantFunc.TypedValue = new DataCompatibility().DefaultValue<T>();
    }
}