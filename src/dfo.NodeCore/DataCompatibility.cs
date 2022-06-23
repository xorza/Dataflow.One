using System;

namespace dfo.NodeCore;

public class DataCompatibility {
    public bool IsValueConvertable(Type funcArgument, Type value) {
        return funcArgument == value || value.IsSubclassOf(funcArgument);
    }

    public object? ConvertValue(object? value, Type targetType) {
        if (targetType.IsValueType && value == null)
            return DefaultValue(targetType);
        return Convert.ChangeType(value, targetType);
    }

    public object? DefaultValue(Type type) {
        if (type.IsValueType) return Activator.CreateInstance(type);

        return null;
    }


    public T? DefaultValue<T>() {
        return default;
    }
}