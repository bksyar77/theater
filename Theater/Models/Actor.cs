
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Theater.Models
{
    public class Actor : INotifyPropertyChanged
    {
        private string _name;
        private string _role;
        private double _balance;

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(nameof(Role)); }
        }

        public double Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(nameof(Balance)); }
        }

        public ObservableCollection<Event> Events { get; set; } = new ObservableCollection<Event>();

        public ObservableCollection<Participation> Participations { get; set; } 
            = new ObservableCollection<Participation>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
