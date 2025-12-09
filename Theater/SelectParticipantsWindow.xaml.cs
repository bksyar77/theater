using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Theater.Models;

namespace Theater
{
    public partial class SelectParticipantsWindow : Window
    {
        private class SelectableActor
        {
            public Actor Actor { get; set; }
            public bool IsSelected { get; set; }
        }

        private readonly ObservableCollection<SelectableActor> _items;

        public List<Actor> SelectedActors { get; private set; } = new List<Actor>();

        public SelectParticipantsWindow(IEnumerable<Actor> allActors, IEnumerable<Actor> alreadySelected)
        {
            InitializeComponent();

            var selectedSet = new HashSet<Actor>(alreadySelected ?? Enumerable.Empty<Actor>());

            _items = new ObservableCollection<SelectableActor>(
                allActors.Select(a => new SelectableActor
                {
                    Actor = a,
                    IsSelected = selectedSet.Contains(a)
                }));

            ActorsListBox.ItemsSource = _items;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedActors = _items
                .Where(x => x.IsSelected && x.Actor != null)
                .Select(x => x.Actor)
                .ToList();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
