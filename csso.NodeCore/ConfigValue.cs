using System.Reflection;
using csso.Common;

namespace csso.NodeCore;

public class ConfigValue {
    private Object? _value;

    public ConfigValue(FunctionConfig config) : this(config, config.DefaultValue) { }

    public ConfigValue(FunctionConfig config, Object? value) {
        Config = config;
        Value = value;
    }

    public FunctionConfig Config { get; }

    public Type Type => Config.Type;

    public Object? Value {
        get => _value;
        set {
            if (value != null)
                Check.True(value.GetType() == Type);

            _value = value;
        }
    }

    internal SerializedConfigValue Serialize() {
        SerializedConfigValue result = new();

        result.Value = Value?.ToString();
        result.ConfigIndex = Config.Index;

        return result;
    }

    public ConfigValue(
        Function func,
        SerializedConfigValue serialized) {
        Config = func.Config.Single(_ => _.Index == serialized.ConfigIndex);

        if (serialized.Value != null
            && StringParser.TryParse(serialized.Value, Type, out Object? value))
            Value = value;
    }
}

public struct SerializedConfigValue {
    public String? Value { get; set; }
    public UInt32 ConfigIndex { get; set; }
}