using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.Json;
using Theater.Models;
using Theater.ViewModel;

namespace Theater
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        private void ActorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedActor = ActorsDataGrid.SelectedItem as Actor;
            }
        }
        private void AddActorTable(object sender, RoutedEventArgs e)
        {
            var dialog = new AddActorWindow();
            dialog.Owner = this; // Устанавливаем владельца

            if (dialog.ShowDialog() == true && dialog.NewActor != null)
            {
                // Получаем ViewModel из DataContext
                if (DataContext is MainViewModel viewModel)
                {
                    // Добавляем нового актера в коллекцию
                    viewModel.Actors.Add(dialog.NewActor);
                }
            }
        }
        private void EditActor(Actor actor)
        {
            if (actor == null) return;

            var dialog = new AddActorWindow();
            dialog.Owner = this;

            // Заполняем поля текущими значениями
            dialog.NameTextBox.Text = actor.Name;
            dialog.RoleTextBox.Text = actor.Role;
            dialog.BalanceTextBox.Text = actor.Balance.ToString();

            if (dialog.ShowDialog() == true && dialog.NewActor != null)
            {
                // Обновляем данные актера
                actor.Name = dialog.NewActor.Name;
                actor.Role = dialog.NewActor.Role;
                actor.Balance = dialog.NewActor.Balance;

                // Обновляем привязку
                if (DataContext is MainViewModel viewModel)
                {
                    // Для обновления UI при изменении существующего объекта
                    var index = viewModel.Actors.IndexOf(actor);
                    if (index >= 0)
                    {
                        viewModel.Actors[index] = actor;
                    }
                }
            }
        }

        // Метод для удаления актера
        private void DeleteActor(Actor actor)
        {
            if (actor == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить актера '{actor.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.Actors.Remove(actor);
                }
            }
        }

        // Обработчики для контекстного меню
        private void EditActorContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ActorsDataGrid.SelectedItem is Actor selectedActor)
            {
                EditActor(selectedActor);
            }
        }

        private void DeleteActorContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ActorsDataGrid.SelectedItem is Actor selectedActor)
            {
                DeleteActor(selectedActor);
            }
        }
        private void AddEventTable(object sender, RoutedEventArgs e)
        {
            AddEvent();
        }

        private void AddEvent()
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel == null || viewModel.SelectedActor == null)
            {
                MessageBox.Show("Сначала выберите актёра, чтобы добавить событие.");
                return;
            }

            var dialog = new AddEventWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && dialog.NewEvent != null)
            {
                viewModel.SelectedActor.Events.Add(dialog.NewEvent);
            }
        }

        // Метод для редактирования события
        private void EditEvent(Event theaterEvent)
        {
            if (theaterEvent == null) return;

            var dialog = new AddEventWindow();
            dialog.Owner = this;

            // Заполняем поля текущими значениями
            dialog.TitleTextBox.Text = theaterEvent.Title;
            dialog.DatePicker.SelectedDate = theaterEvent.Date;
            dialog.TimeTextBox.Text = theaterEvent.Date.ToString("HH:mm");

            // Устанавливаем тип
            foreach (ComboBoxItem item in dialog.TypeComboBox.Items)
            {
                if (item.Content?.ToString() == theaterEvent.Type)
                {
                    dialog.TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            dialog.Title = "Редактировать событие";

            if (dialog.ShowDialog() == true && dialog.NewEvent != null)
            {
                // Обновляем данные события
                theaterEvent.Title = dialog.NewEvent.Title;
                theaterEvent.Date = dialog.NewEvent.Date;
                theaterEvent.Type = dialog.NewEvent.Type;
            }
        }

        // Метод для удаления события
        private void DeleteEvent(Event theaterEvent)
        {
            if (theaterEvent == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить событие '{theaterEvent.Title}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.SelectedActor?.Events.Remove(theaterEvent);
                }
            }
        }

        // Обработчики для контекстного меню событий
        private void EditEventContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (EventsDataGrid.SelectedItem is Event selectedEvent)
                {
                    EditEvent(selectedEvent);
                }
                else
                {
                    MessageBox.Show("Выберите событие для редактирования");
                }
            }
        }

        private void DeleteEventContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (EventsDataGrid.SelectedItem is Event selectedEvent)
                {
                    DeleteEvent(selectedEvent);
                }
                else
                {
                    MessageBox.Show("Выберите событие для удаления");
                }
            }
        }
        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Сохранить данные театра"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(viewModel.Actors, options);
                    File.WriteAllText(dialog.FileName, json);

                    MessageBox.Show("Данные успешно сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel == null)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Загрузить данные театра"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    var loadedActors = JsonSerializer.Deserialize<ObservableCollection<Actor>>(json);

                    if (loadedActors != null)
                    {
                        viewModel.Actors.Clear();
                        foreach (var actor in loadedActors)
                            viewModel.Actors.Add(actor);

                        MessageBox.Show("Данные успешно загружены!", "Загрузка", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

