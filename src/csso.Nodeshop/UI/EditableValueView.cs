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

    public virtual bool HasValue {
        get => true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static EditableValueView Create(NodeArg inputArg) {
        var editableValueView =
            typeof(EditableValueView<>)
                .MakeGenericType(inputArg.Type)
                .GetConstructors()
                .First()
                .Invoke(new object[] {inputArg});
        return (EditableValueView) editableValueView;
    }

    public static EditableValueView Create(ConstantFunc func) {
        var editableValueView =
            typeof(ConstantValueFuncView<>)
                .MakeGenericType(func.Type)
                .GetConstructors()
                .First()
                .Invoke(new object[] {func});
        return (EditableValueView) editableValueView;
    }

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected abstract void ResetValue();
}

public class EditableValueView<T> : EditableValueView {
    public NodeArg InputArg { get; }

    public DataSubscription? DataSubscription {
        get => InputArg.Node.Graph.GetDataSubscription(InputArg);
    }

    public EditableValueView(NodeArg inputArg) : base(inputArg.Type) {
        Check.Argument(inputArg.ArgDirection == ArgDirection.In, nameof(inputArg));

        InputArg = inputArg;
    }

    public T? Value {
        get {
            if (DataSubscription == null) {
                return default;
            }

            return (T?) DataSubscription.Value;
        }
        set {
            if (Equals(value, DataSubscription?.Value)) {
                return;
            }

            if (value == null) {
                InputArg.Node.Graph.RemoveSubscription(InputArg);
            } else {
                InputArg.Node.Graph.Add(new DataSubscription(InputArg, value));
            }

            OnPropertyChanged();
        }
    }

    public override bool HasValue {
        get => DataSubscription?.Value != null;
    }

    public bool HasSource {
        get => DataSubscription?.Source != null;
    }

    protected override void ResetValue() {
        InputArg.Node.Graph.RemoveSubscription(InputArg);
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