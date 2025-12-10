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
using System.Text.Json.Serialization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Word = DocumentFormat.OpenXml.Wordprocessing; // <--- ВАЖНО



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

        private void ActorStatsContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ActorsDataGrid.SelectedItem is Actor selectedActor)
            {
                var statsWindow = new ActorStatsWindow(selectedActor);
                statsWindow.Owner = this;
                statsWindow.ShowDialog();
            }
        }
        private void AddEventTable(object sender, RoutedEventArgs e)
        {
            AddEvent();
        }

        private void AddEvent()
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel == null)
                return;

            var dialog = new AddEventWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && dialog.NewEvent != null)
            {
                var newEvent = dialog.NewEvent;

                // Добавляем событие в глобальный список
                viewModel.Events.Add(newEvent);

                // Если выбран актёр — сразу создаём участие для него
                if (viewModel.SelectedActor != null)
                {
                    var actor = viewModel.SelectedActor;

                    if (actor.Events == null)
                        actor.Events = new ObservableCollection<Event>();

                    if (!actor.Events.Contains(newEvent))
                        actor.Events.Add(newEvent);

                    var participation = new Participation
                    {
                        Actor = actor,
                        Event = newEvent
                    };

                    newEvent.Participations.Add(participation);
                    actor.Participations.Add(participation);
                }

                viewModel.EventsView.Refresh();
            }
        }

        // Метод для редактирования события
        private void EditEvent(Event theaterEvent)
        {
            if (theaterEvent == null)
                return;

            if (DataContext is MainViewModel viewModel)
            {
                var dialog = new EditEventWindow(theaterEvent, viewModel.Actors);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    // Объект theaterEvent уже обновлён окном
                    viewModel.EventsView.Refresh();
                }
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
                    // Удаляем из глобального списка событий
                    viewModel.Events.Remove(theaterEvent);

                    // Удаляем участие и событие у всех актёров
                    foreach (var actor in viewModel.Actors)
                    {
                        // Удаляем Participation
                        var participationsToRemove = actor.Participations
                            .Where(p => p.Event == theaterEvent)
                            .ToList();

                        foreach (var p in participationsToRemove)
                        {
                            actor.Participations.Remove(p);
                            theaterEvent.Participations.Remove(p);
                        }

                        // Удаляем событие из списка событий актёра
                        actor.Events?.Remove(theaterEvent);
                    }

                    viewModel.EventsView.Refresh();
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
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        ReferenceHandler = ReferenceHandler.Preserve
                    };

                    string json = JsonSerializer.Serialize(viewModel.Actors, options);
                    File.WriteAllText(dialog.FileName, json);


                    MessageBox.Show("Данные успешно сохранены", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    };

                    string json = File.ReadAllText(dialog.FileName);
                    var loadedActors = JsonSerializer.Deserialize<ObservableCollection<Actor>>(json, options);


                    if (loadedActors != null)
                    {
                        viewModel.Actors.Clear();
                        foreach (var actor in loadedActors)
                            viewModel.Actors.Add(actor);

                        // Перестраиваем глобальный список событий и участия
                        viewModel.Events.Clear();

                        foreach (var actor in viewModel.Actors)
                        {
                            if (actor.Events != null)
                            {
                                foreach (var ev in actor.Events)
                                {
                                    if (!viewModel.Events.Contains(ev))
                                    {
                                        viewModel.Events.Add(ev);
                                    }

                                    // Гарантируем наличие Participation для связки актёр-событие
                                    if (!actor.Participations.Any(p => p.Event == ev))
                                    {
                                        var participation = new Participation
                                        {
                                            Actor = actor,
                                            Event = ev
                                        };
                                        actor.Participations.Add(participation);
                                        ev.Participations.Add(participation);
                                    }
                                }
                            }
                        }

                        viewModel.EventsView.Refresh();

                        MessageBox.Show("Данные успешно загружены", "Загрузка", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExportGlobalStats_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null)
                return;
            var win = new ExportPeriodWindow(viewModel);
            win.Owner = this;
            win.ShowDialog();
        }

        private void ExportActorsTableToWord(string filename)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null)
                return;
            var win = new ExportPeriodWindow(viewModel);
            win.Owner = this;
            win.ShowDialog();
            using (var word = WordprocessingDocument.Create(filename, WordprocessingDocumentType.Document))
            {
                var mainPart = word.AddMainDocumentPart();
                mainPart.Document = new Word.Document();
                var body = new Word.Body();

                // Заголовок документа
                var headingRun = new Word.Run(new Word.Text("Общая статистика всех актёров"));
                var headingParagraph = new Word.Paragraph(headingRun)
                {
                    ParagraphProperties = new Word.ParagraphProperties(
                        new Word.Justification { Val = Word.JustificationValues.Center })
                };

                body.Append(headingParagraph);
                body.Append(new Word.Paragraph(new Word.Run(new Word.Text("")))); // пустая строка

                // Список всех типов событий
                var eventTypes = viewModel.Events
                    .Select(ev => ev.Type ?? "Без типа")
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                // Таблица
                var table = new Word.Table();

                var tblProps = new Word.TableProperties(
                    new Word.TableBorders(
                        new Word.TopBorder { Val = Word.BorderValues.Single, Size = 4 },
                        new Word.BottomBorder { Val = Word.BorderValues.Single, Size = 4 },
                        new Word.LeftBorder { Val = Word.BorderValues.Single, Size = 4 },
                        new Word.RightBorder { Val = Word.BorderValues.Single, Size = 4 },
                        new Word.InsideHorizontalBorder { Val = Word.BorderValues.Single, Size = 4 },
                        new Word.InsideVerticalBorder { Val = Word.BorderValues.Single, Size = 4 }
                    )
                );
                table.AppendChild(tblProps);

                // Заголовочная строка: первая колонка "Актёр", далее — типы событий
                var headerRow = new Word.TableRow();
                headerRow.Append(MakeWordCell("Актёр", bold: true));
                foreach (var type in eventTypes)
                {
                    headerRow.Append(MakeWordCell(type, bold: true));
                }
                table.Append(headerRow);

                // Строки: актёры
                foreach (var actor in viewModel.Actors)
                {
                    var row = new Word.TableRow();

                    // Имя актёра
                    row.Append(MakeWordCell(actor.Name));

                    // По каждому типу события — часы и участия
                    foreach (var type in eventTypes)
                    {
                        var participations = actor.Participations
                            .Where(p => p.Event != null && (p.Event.Type ?? "Без типа") == type)
                            .ToList();

                        int count = participations.Count;
                        double hours = participations.Sum(p => p.Hours);

                        string cellText = $"{hours:N2} ч ({count} уч.)";

                        row.Append(MakeWordCell(cellText));
                    }

                    table.Append(row);
                }

                body.Append(table);
                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
        }
        private Word.TableCell MakeWordCell(string text, bool bold = false)
        {
            var run = new Word.Run();

            if (bold)
            {
                var runProps = new Word.RunProperties(new Word.Bold());
                run.Append(runProps);
            }

            run.Append(new Word.Text(text ?? string.Empty));

            var paragraph = new Word.Paragraph(run);
            var cell = new Word.TableCell(paragraph);
            return cell;
        }



    }
}