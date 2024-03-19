using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestHost.Annotations;

namespace TestHost;

//public class BindingEntity : INotifyPropertyChanged
//{
//    private DateTime _time = DateTime.Now;
//    private DateTime? _timeNull;
//    private decimal _value;
//    private int _intValue;
//    public string Message { get; set; }

//    public decimal Value
//    {
//        get => _value;
//        set
//        {
//            _value = value;
//            OnPropertyChanged();
//        }
//    }

//    public DateTime Time
//    {
//        get => _time;
//        set
//        {
//            _time = value;
//            OnPropertyChanged();
//        }
//    }

//    public DateTime? TimeNull
//    {
//        get => _timeNull;
//        set
//        {
//            _timeNull = value;
//            OnPropertyChanged();
//        }
//    }

//    public int IntValue
//    {
//        get => _intValue;
//        set
//        {
//            _intValue = value;
//            OnPropertyChanged();
//        }
//    }

//    public BindingList<Inner> Inners { get; set; } = new();

//    public event PropertyChangedEventHandler PropertyChanged;

//    [NotifyPropertyChangedInvocator]
//    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
//    {
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//    }

//    public override string ToString()
//    {
//        return $"{Message} {Value} {Time} {TimeNull} {IntValue} {string.Join(',', Inners)}";
//    }
//}

//public class BindingEntity : INotifyPropertyChanged
//{

//    public event PropertyChangedEventHandler PropertyChanged;
//    public string Message { get; set; }

//    public decimal Value { get; set; }

//    public DateTime Time { get; set; } =  new(1970,1,1);

//    public DateTime? TimeNull { get; set; }

//    public int IntValue { get; set; }
//    public BindingList<Inner> Inners { get; set; } = new();

//    public override string ToString()
//    {
//        return $"({Message}),({Value}),({Time}),({TimeNull}),({IntValue}) | {string.Join(',', Inners)}";
//    }
//}

//public class Inner
//{
//    public string Key { get; set; }

//    public override string ToString()
//    {
//        return $"{Key}";
//    }
//}

public class BindingEntity : INotifyPropertyChanged
{
    private Inner _inner = new();
    private int _intValue;
    private string _message;
    private DateTime _time = new(1970, 1, 1);
    private DateTime? _timeNull;
    private decimal _value;

    public string Message
    {
        get => _message;
        set
        {
            if (value == _message)
                return;
            _message = value;
            OnPropertyChanged();
        }
    }

    public decimal Value
    {
        get => _value;
        set
        {
            if (value == _value)
                return;
            _value = value;
            OnPropertyChanged();
        }
    }

    public DateTime Time
    {
        get => _time;
        set
        {
            if (value.Equals(_time))
                return;
            _time = value;
            OnPropertyChanged();
        }
    }

    public DateTime? TimeNull
    {
        get => _timeNull;
        set
        {
            if (Nullable.Equals(value, _timeNull))
                return;
            _timeNull = value;
            OnPropertyChanged();
        }
    }

    public int IntValue
    {
        get => _intValue;
        set
        {
            if (value == _intValue)
                return;
            _intValue = value;
            OnPropertyChanged();
        }
    }

    public BindingList<Inner> Inners { get; set; } = new();

    public Inner Inner
    {
        get => _inner;
        set
        {
            if (Equals(value, _inner))
                return;
            _inner = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public override string ToString()
    {
        return $"({Message}),({Value}),({Time}),({TimeNull}),({IntValue}),({Inner})| {string.Join(',', Inners)}";
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Inner
{
    public string Key { get; set; }
    public string Value { get; set; }

    public override string ToString()
    {
        return $"{Key}={Value}";
    }
}