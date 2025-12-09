using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Theater.Models;

namespace Theater
{
    public partial class EditEventWindow : Window
    {
        private readonly Event _event;
        private readonly ObservableCollection<Actor> _allActors;

        public EditEventWindow(Event ev, ObservableCollection<Actor> allActors)
        {
            InitializeComponent();

            _event = ev;
            _allActors = allActors;

            TitleTextBox.Text = _event.Title;
            DatePicker.SelectedDate = _event.Date.Date;
            TimeTextBox.Text = _event.Date.ToString("HH:mm");
            HoursTextBox.Text = _event.Hours.ToString("0.##");

            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Content?.ToString() == _event.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            ParticipantsDataGrid.ItemsSource = _event.Participations;
        }

        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            var existingActors = _event.Participations
                .Where(p => p.Actor != null)
                .Select(p => p.Actor)
                .ToList();

            var dialog = new SelectParticipantsWindow(_allActors, existingActors);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && dialog.SelectedActors.Any())
            {
                foreach (var actor in dialog.SelectedActors)
                {
                    if (!_event.Participations.Any(p => p.Actor == actor))
                    {
                        var participation = new Participation
                        {
                            Actor = actor,
                            Event = _event
                        };

                        _event.Participations.Add(participation);
                        actor.Participations.Add(participation);

                        if (!actor.Events.Contains(_event))
                            actor.Events.Add(_event);
                    }
                }
            }
        }

        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (ParticipantsDataGrid.SelectedItem is Participation participation)
            {
                var result = MessageBox.Show(
                    $"Удалить участника '{participation.Actor?.Name}' из события?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _event.Participations.Remove(participation);

                    if (participation.Actor != null)
                    {
                        participation.Actor.Participations.Remove(participation);

                        if (!participation.Actor.Participations.Any(p => p.Event == _event))
                        {
                            participation.Actor.Events.Remove(_event);
                        }
                    }
                }
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название события", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fullDate;
            if (!DateTime.TryParse($"{DatePicker.SelectedDate.Value.ToShortDateString()} {TimeTextBox.Text}", out fullDate))
            {
                MessageBox.Show("Введите корректное время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var typeItem = TypeComboBox.SelectedItem as ComboBoxItem;
            string type = typeItem?.Content?.ToString() ?? "Спектакль";

            double hours;
            if (!double.TryParse(HoursTextBox.Text.Replace(",", "."),
                                 System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out hours))
            {
                hours = 0;
            }

            _event.Title = TitleTextBox.Text.Trim();
            _event.Date = fullDate;
            _event.Type = type;
            _event.Hours = hours;

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
