
using System;
using System.ComponentModel;

namespace Theater.Models
{
    public class Participation : INotifyPropertyChanged
    {
        private double _hours;
        private bool _overrideHours;

        public int Id { get; set; }

        public Actor Actor { get; set; }
        public Event Event { get; set; }

        public double Hours
        {
            get => _overrideHours ? _hours : Event?.Hours ?? 0;
            set
            {
                _overrideHours = true;
                _hours = value;
                OnPropertyChanged(nameof(Hours));
            }
        }

        public bool OverrideHours
        {
            get => _overrideHours;
            set
            {
                _overrideHours = value;
                OnPropertyChanged(nameof(OverrideHours));
                OnPropertyChanged(nameof(Hours));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
