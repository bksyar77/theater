using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using Theater.Models;

namespace Theater.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Actor> Actors { get; set; } = new ObservableCollection<Actor>();

        public ObservableCollection<Event> Events { get; set; } = new ObservableCollection<Event>();

        public ICollectionView EventsView { get; }

        public MainViewModel()
        {
            EventsView = CollectionViewSource.GetDefaultView(Events);
            EventsView.Filter = EventFilter;
        }

        private bool EventFilter(object obj)
        {
            if (obj is Event ev)
            {
                if (SelectedActor == null) return true;
                return SelectedActor.Events != null && SelectedActor.Events.Contains(ev);
            }
            return false;
        }

        private Actor _selectedActor;
        public Actor SelectedActor
        {
            get => _selectedActor;
            set
            {
                _selectedActor = value;
                OnPropertyChanged();
                EventsView?.Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}