using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Theater.Models;
using Microsoft.Win32;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;


namespace Theater
{
    public partial class ActorStatsWindow : Window
    {
        private class StatsItem
        {
            public string EventType { get; set; }
            public int ParticipationCount { get; set; }
            public double TotalHours { get; set; }
        }

        private readonly Actor _actor;

        public ActorStatsWindow(Actor actor)
        {
            InitializeComponent();
            _actor = actor ?? throw new ArgumentNullException(nameof(actor));
            LoadStats();
        }

        private void LoadStats()
        {
            ActorNameTextBlock.Text = $"Актёр: {_actor.Name}";

            var start = StartDatePicker.SelectedDate;
            var end = EndDatePicker.SelectedDate;

            // Базовый источник: все участия
            var participations = _actor.Participations
                .Where(p => p.Event != null);

            // Фильтр по дате "С"
            if (start != null)
                participations = participations.Where(p => p.Event.Date >= start.Value);

            // Фильтр по дате "По"
            if (end != null)
                participations = participations.Where(p => p.Event.Date <= end.Value);

            // Группировка по типам событий
            var groups = participations
                .GroupBy(p => p.Event.Type ?? "Без типа")
                .Select(g => new StatsItem
                {
                    EventType = g.Key,
                    ParticipationCount = g.Count(),
                    TotalHours = g.Sum(p => p.Hours)
                })
                .OrderByDescending(x => x.TotalHours)
                .ToList();

            StatsDataGrid.ItemsSource = groups;

            var totalPart = groups.Sum(g => g.ParticipationCount);
            var totalHours = groups.Sum(g => g.TotalHours);

            TotalsTextBlock.Text = $"Всего участий: {totalPart}, всего часов: {totalHours:N2}";
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadStats();
        }
        private void SaveToWord_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить статистику актёра"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                using (var word = WordprocessingDocument.Create(
                    dialog.FileName, WordprocessingDocumentType.Document))
                {
                    var mainPart = word.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = new Body();

                    // Заголовок
                    body.Append(
                        new Paragraph(
                            new Run(
                                new Text($"Статистика актёра: {_actor.Name}")))
                        {
                            ParagraphProperties = new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Center })
                        }
                    );

                    body.Append(new Paragraph(new Run(new Text($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}"))));
                    body.Append(new Paragraph(new Run(new Text("")))); // пустая строка

                    // Создаем таблицу
                    var table = new Table();

                    // Стиль таблицы
                    var props = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 4 },
                            new BottomBorder { Val = BorderValues.Single, Size = 4 },
                            new LeftBorder { Val = BorderValues.Single, Size = 4 },
                            new RightBorder { Val = BorderValues.Single, Size = 4 },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                        )
                    );

                    table.AppendChild(props);

                    // Заголовки колонок
                    var headerRow = new TableRow();
                    headerRow.Append(
                        MakeCell("Тип события", true),
                        MakeCell("Участий", true),
                        MakeCell("Часы", true)
                    );
                    table.Append(headerRow);

                    // Добавление строк данных (берем их из UI DataGrid)
                    foreach (var item in StatsDataGrid.Items)
                    {
                        if (item is StatsItem stat)
                        {
                            var row = new TableRow();
                            row.Append(
                                MakeCell(stat.EventType),
                                MakeCell(stat.ParticipationCount.ToString()),
                                MakeCell(stat.TotalHours.ToString("N2"))
                            );
                            table.Append(row);
                        }
                    }


                    body.Append(table);
                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }

                MessageBox.Show("Отчёт успешно сохранён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения Word: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private TableCell MakeCell(string text, bool bold = false)
        {
            Run run = bold ? new Run(new RunProperties(new Bold()), new Text(text))
                           : new Run(new Text(text));

            return new TableCell(new Paragraph(run));
        }


    }
}
