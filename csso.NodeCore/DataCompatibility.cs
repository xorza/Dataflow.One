namespace csso.NodeCore;

public class DataCompatibility {
    public static bool IsValueConvertable(Type funcArgument, Type value) {
        return funcArgument == value || value.IsSubclassOf(funcArgument);
    }

    public static Object? ConvertValue(Object? value, Type targetType) {
        if (targetType.IsValueType && value == null) {
            return DefaultValue(targetType);
        } else {
            return Convert.ChangeType(value, targetType);
        }
    }

    public static Object? DefaultValue(Type type) {
        if (type.IsValueType) {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    public static T? DefaultValue<T>() {
        return default(T);
    }
}