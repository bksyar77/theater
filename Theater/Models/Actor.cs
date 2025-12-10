
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;

namespace Theater.Models
{
    public class Actor : INotifyPropertyChanged
    {
        public Actor()
        {
            Participations = new ObservableCollection<Participation>();
        }

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

        /// <summary>
        /// Суммарные часы актёра (на основе участий в событиях).
        /// </summary>
        public double Balance
        {
            get => Participations != null ? Participations.Sum(p => p.Hours) : 0;
            set
            {
                // Значение задаётся автоматически через участия; ручная установка сохраняется в поле,
                // но на расчёт не влияет.
                _balance = value;
                OnPropertyChanged(nameof(Balance));
            }
        }

        public ObservableCollection<Event> Events { get; set; } = new ObservableCollection<Event>();

        private ObservableCollection<Participation> _participations = new ObservableCollection<Participation>();

        public ObservableCollection<Participation> Participations
        {
            get => _participations;
            set
            {
                if (_participations != null)
                {
                    _participations.CollectionChanged -= Participations_CollectionChanged;
                    foreach (var p in _participations)
                        p.PropertyChanged -= Participation_PropertyChanged;
                }

                _participations = value ?? new ObservableCollection<Participation>();
                _participations.CollectionChanged += Participations_CollectionChanged;

                foreach (var p in _participations)
                    p.PropertyChanged += Participation_PropertyChanged;

                OnPropertyChanged(nameof(Participations));
                OnPropertyChanged(nameof(Balance));
            }
        }

        private void Participations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Participation p in e.NewItems)
                    p.PropertyChanged += Participation_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (Participation p in e.OldItems)
                    p.PropertyChanged -= Participation_PropertyChanged;
            }

            OnPropertyChanged(nameof(Balance));
        }

        private void Participation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Participation.Hours) || e.PropertyName == nameof(Participation.OverrideHours))
            {
                OnPropertyChanged(nameof(Balance));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)  
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
