using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using dfo.NodeCore.Annotations;

namespace dfo.NodeRunner.UI;

public class ValueView : INotifyPropertyChanged {
    private static readonly Dictionary<Type, Func<ValueView>> Factory = new();
    private bool _isLoading;

    private object? _value;

    static ValueView() { }

    public ValueView(PutView putView, object? value) {
        IsLoading = false;
        Value = value;
        PutView = putView;
    }

    public PutView PutView { get; }

    public object? Value {
        get => _value;
        set {
            if (value == _value) {
                return;
            }

            _value = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading {
        get => _isLoading;
        set {
            if (_isLoading == value) {
                return;
            }

            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasValue));
        }
    }

    public bool HasValue => !IsLoading;
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static ValueView FromValue(PutView putView, object? value) {
        if (value == null) {
            return new NullValueView(putView);
        }

        if (Factory.TryGetValue(value.GetType(), out var factory)) {
            return factory!.Invoke();
        }

        return new ValueView(putView, value);
    }
}

internal class NullValueView : ValueView {
    public NullValueView(PutView putView) : base(putView, "null") { }
}