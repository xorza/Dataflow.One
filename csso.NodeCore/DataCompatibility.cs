using System;

namespace csso.NodeCore;

public class DataCompatibility {
    public bool IsValueConvertable(Type funcArgument, Type value) {
        return funcArgument == value || value.IsSubclassOf(funcArgument);
    }

    public Object? ConvertValue(Object? value, Type targetType) {
        if (targetType.IsValueType && value == null) {
            return DefaultValue(targetType);
        } else {
            return Convert.ChangeType(value, targetType);
        }
    }

    public Object? DefaultValue(Type type) {
        if (type.IsValueType) {
            return Activator.CreateInstance(type);
        }

        return null;
    }
    

    public T? DefaultValue<T>() {
        return default(T);
    }
}