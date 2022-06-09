namespace csso.NodeCore; 

public class DataCompatibility {
    public  static  bool CanAssignValue(Type funcArgument, Type value) {
        return funcArgument == value || value.IsSubclassOf(funcArgument);
    }
}