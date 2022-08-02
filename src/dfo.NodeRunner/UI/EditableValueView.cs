using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using dfo.NodeCore;
using dfo.NodeCore.Annotations;
using dfo.NodeCore.Funcs;

namespace dfo.NodeRunner.UI;

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

    public static EditableValueView Create(ConstantFunc func) {
        var editableValueView =
            typeof(EditableValueView<>)
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
    private readonly ConstantFunc<T> _constantFunc;

    public EditableValueView(ConstantFunc<T> constantFunc) : base(typeof(T)) {
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


// public Object Target { get; }
// public PropertyInfo Property { get; }
// public EditableValueView(Object target, PropertyInfo property) {
//     Type = property.PropertyType;
//     Target = target;
//     Property = property;
// }
// public static EditableValueView Create<TSource, TProperty>(
//     TSource target,
//     Expression<Func<TSource, TProperty>> propertyLambda) {
//     Type type = typeof(TSource);
//
//     MemberExpression? member = propertyLambda.Body as MemberExpression;
//     if (member == null) {
//         throw new ArgumentException(
//             $"Expression '{propertyLambda.ToString()}' refers to a method, not a property."
//         );
//     }
//
//     PropertyInfo? propInfo = member.Member as PropertyInfo;
//     if (propInfo == null) {
//         throw new ArgumentException(
//             $"Expression '{propertyLambda.ToString()}' refers to a field, not a property."
//         );
//     }
//
//     if (type != propInfo.ReflectedType &&
//         !type.IsSubclassOf(propInfo.ReflectedType!)) {
//         throw new ArgumentException(string.Format(
//             "Expression '{0}' refers to a property that is not from type {1}.",
//             propertyLambda.ToString(),
//             type));
//     }
//
//     return new EditableValueView(target!, propInfo);
// }
// public object? Value {
//     get => _value;
//     set {
//         if (value == _value) return;
//         
//         var converted = DataCompatibility.ConvertValue(value, Type);
//         _value = converted;
//         Property.SetValue(Target, converted);
//         OnPropertyChanged();
//     }
// }