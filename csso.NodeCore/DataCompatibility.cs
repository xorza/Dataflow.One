namespace csso.NodeCore;

public class DataCompatibility {
    public static bool CanAssignValue(Type funcArgument, Type value) {
        return funcArgument == value || value.IsSubclassOf(funcArgument);
    }

    public static Object ConvertValue(Object value, Type targetType) {
        return Convert.ChangeType(value, targetType);
    }
}