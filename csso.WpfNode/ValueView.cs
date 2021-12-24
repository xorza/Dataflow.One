using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.NodeCore.Annotations;

namespace csso.WpfNode;

public class ValueView : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private object? _value;

    public object? Value {
        get => _value;
        set {
            if (value == _value) return;
            _value = value;
            OnPropertyChanged();
        }
    }

    private bool _isLoading = false;

    public bool IsLoading {
        get => _isLoading;
        set {
            if (_isLoading == value) return;

            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasValue));
        }
    }

    public bool HasValue => !IsLoading;

    public ValueView() {
        IsLoading = false;
        Value = "hi123";
        
    }
    public ValueView(object? value) {
        IsLoading = false;
        Value = value;
        
    }
}