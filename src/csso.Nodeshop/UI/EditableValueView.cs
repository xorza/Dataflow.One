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
    private bool _hasValue;

    protected EditableValueView(Type type) {
        Type = type;
    }

    public Type Type { get; }

    public bool HasValue {
        get => _hasValue;
        set {
            if (_hasValue == value) {
                return;
            }

            if (!value) {
                ResetValue();
            }

            _hasValue = value;
            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    public static EditableValueView Create(DataSubscription dataSubscription) {
        var editableValueView =
            typeof(EditableValueView<>)
                .MakeGenericType(dataSubscription.Subscriber.Type)
                .GetConstructors()
                .First()
                .Invoke(new object[] {});
        return (EditableValueView) editableValueView;
    }
    
    public static EditableValueView Create(ConstantFunc func) {
        var editableValueView =
            typeof(ConstantFuncView<>)
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
    
    public DataSubscription DataSubscription { get; }

    public EditableValueView(DataSubscription dataSubscription) : base(dataSubscription.Subscriber.Type) {
        DataSubscription = dataSubscription;
    }

    public T? Value {
        get => (T?)DataSubscription.Value;
        set {
            if (EqualityComparer<T>.Default.Equals(value, (T?)DataSubscription.Value)) {
                return;
            }
    
            DataSubscription.Value = value;
            OnPropertyChanged();
        }
    }
    
    protected override void ResetValue() {
        DataSubscription.Value =  DataCompatibility.Instance.DefaultValue<T>();
    }
}

public class ConstantFuncView<T> : EditableValueView {
    private readonly ConstantFunc<T> _constantFunc;

    public ConstantFuncView(ConstantFunc<T> constantFunc) : base(typeof(T)) {
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
