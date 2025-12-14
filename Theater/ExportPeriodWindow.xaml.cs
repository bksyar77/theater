using System;
using System.Linq;
using System.Windows;
using Theater.ViewModel;

namespace Theater
{
    public partial class ExportPeriodWindow : Window
    {
        private readonly MainViewModel _vm;
        private readonly ISave _saveService = new WordSaveService();

        public ExportPeriodWindow(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите обе даты.");
                return;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата 'С' не может быть позже даты 'По'.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = "Статистика_за_период.docx"
            };

            if (dialog.ShowDialog() != true)
                return;

            var table = BuildTable();
            _saveService.Save(dialog.FileName, table);

            MessageBox.Show("Файл успешно сохранён!");
            Close();
        }

        private SaveTable BuildTable()
        {
            var start = StartDatePicker.SelectedDate.Value;
            var end = EndDatePicker.SelectedDate.Value;

            var types = _vm.Events
                .Where(e => e.Date >= start && e.Date <= end)
                .Select(e => e.Type ?? "Без типа")
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var table = new SaveTable
            {
                Title = $"Статистика за период {start:dd.MM.yyyy} — {end:dd.MM.yyyy}"
            };

            table.Columns.Add("Актёр");
            foreach (var t in types)
                table.Columns.Add(t);

            foreach (var actor in _vm.Actors)
            {
                var row = new System.Collections.Generic.List<string>();
                row.Add(actor.Name);

                foreach (var t in types)
                {
                    var parts = actor.Participations
                        .Where(p => p.Event.Type == t && p.Event.Date >= start && p.Event.Date <= end)
                        .ToList();

                    var hours = parts.Sum(p => p.Hours);
                    var participates = parts.Count(p =>
                        p.Event.Type == "Спектакль" ||
                        p.Event.Type == "Спектакль на выезде" ||
                        p.Event.Type == "Спектакль на стационаре");

                    row.Add($"{hours:N1} ч ({participates} уч.)");
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
