using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using csso.NodeCore;
using csso.NodeCore.Annotations;

namespace csso.WpfNode;

public class EditableValueView : INotifyPropertyChanged {
    private object? _value = null;
    private bool _hasValue = false;

    public Object Target { get; }
    public Type Type { get; }
    public PropertyInfo Property { get; }

    public EditableValueView(Object target, PropertyInfo property) {
        Type = property.PropertyType;
        Target = target;
        Property = property;
    }


    public static EditableValueView Create<TSource, TProperty>(
        TSource target,
        Expression<Func<TSource, TProperty>> propertyLambda) {
        Type type = typeof(TSource);

        MemberExpression? member = propertyLambda.Body as MemberExpression;
        if (member == null) {
            throw new ArgumentException(
                $"Expression '{propertyLambda.ToString()}' refers to a method, not a property."
            );
        }

        PropertyInfo? propInfo = member.Member as PropertyInfo;
        if (propInfo == null) {
            throw new ArgumentException(
                $"Expression '{propertyLambda.ToString()}' refers to a field, not a property."
            );
        }

        if (type != propInfo.ReflectedType &&
            !type.IsSubclassOf(propInfo.ReflectedType!)) {
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a property that is not from type {1}.",
                propertyLambda.ToString(),
                type));
        }

        return new EditableValueView(target!, propInfo);
    }

    public object? Value {
        get => _value;
        set {
            if (value == _value) return;

            _value = value;
            
            var converted = DataCompatibility.ConvertValue(_value!, Type);
            Property.SetValue(Target, converted);
            OnPropertyChanged();
        }
    }

    public bool HasValue {
        get => _hasValue;
        set {
            if (_hasValue == value) return;
            if (!value) {
                Value = null;
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
}