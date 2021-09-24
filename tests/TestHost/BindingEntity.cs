using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestHost.Annotations;

namespace TestHost
{
    public class BindingEntity : INotifyPropertyChanged
    {
        private DateTime _time = DateTime.Now;
        private DateTime? _timeNull;
        private decimal _value;
        public string Message { get; set; }

        public decimal Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        public DateTime? TimeNull
        {
            get => _timeNull;
            set
            {
                _timeNull = value;
                OnPropertyChanged();
            }
        }

        public BindingList<Inner> Inners { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Message} {Value} {Time} {TimeNull} {string.Join(',', Inners)}";
        }
    }

    public class Inner
    {
        public string Key { get; set; }

        public override string ToString()
        {
            return $"{Key}";
        }
    }
}