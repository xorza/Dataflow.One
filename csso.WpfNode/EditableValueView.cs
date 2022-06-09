﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using csso.NodeCore;
using csso.NodeCore.Annotations;
using csso.NodeCore.Funcs;

namespace csso.WpfNode;

public abstract class EditableValueView : INotifyPropertyChanged {
    private bool _hasValue = false;

    public Type Type { get; }

    protected EditableValueView(Type type) {
        Type = type;
    }

    public bool HasValue {
        get => _hasValue;
        set {
            if (_hasValue == value) return;
            if (!value) {
                ResetValue();
            }

            _hasValue = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected abstract void ResetValue();
}

public class EditableValueView<T> : EditableValueView {
    private readonly ValueFunc<T> _valueFunc;

    public EditableValueView(ValueFunc<T> valueFunc) : base(typeof(T)) {
        _valueFunc = valueFunc;
    }

    public T? Value {
        get => _valueFunc.TypedValue;
        set {
            if (EqualityComparer<T>.Default.Equals(value, _valueFunc.TypedValue)) {
                return;
            }

            _valueFunc.TypedValue = value;
            OnPropertyChanged();
        }
    }

    protected override void ResetValue() {
        _valueFunc.TypedValue = DataCompatibility.DefaultValue<T>();
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
