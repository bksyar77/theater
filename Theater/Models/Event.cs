
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Theater.Models
{
    public class Event : INotifyPropertyChanged
    {
        private string _title;
        private DateTime _date;
        private string _type;
        private double _hours;

        public int Id { get; set; }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public double Hours
        {
            get => _hours;
            set { _hours = value; OnPropertyChanged(nameof(Hours)); }
        }

        public ObservableCollection<Participation> Participations { get; set; } 
            = new ObservableCollection<Participation>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
