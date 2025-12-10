using System;
using System.Linq;
using System.Windows;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Theater.ViewModel;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Theater
{
    public partial class ExportPeriodWindow : Window
    {
        private readonly MainViewModel _vm;

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
            if (StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null)
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

            ExportStats(dialog.FileName);
            MessageBox.Show("Файл успешно сохранён!");
            Close();
        }

        private bool IsParticipation(string type) =>
            type == "Спектакль" ||
            type == "Спектакль на выезде" ||
            type == "Спектакль на стационаре";

        private Word.TableCell Cell(string text)
        {
            var p = new Word.Paragraph(new Word.Run(new Word.Text(text ?? "")));
            return new Word.TableCell(p);
        }

        private void ExportStats(string filename)
        {
            var start = StartDatePicker.SelectedDate.Value;
            var end = EndDatePicker.SelectedDate.Value;

            var eventTypes = _vm.Events
                .Where(ev => ev.Date >= start && ev.Date <= end)
                .Select(ev => ev.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var doc = WordprocessingDocument.Create(
                filename, WordprocessingDocumentType.Document);

            var main = doc.AddMainDocumentPart();
            main.Document = new Word.Document();
            var body = new Word.Body();

            // Заголовок
            var title = new Word.Paragraph(
                new Word.Run(
                    new Word.RunProperties(new Word.Bold()),
                    new Word.Text($"Статистика за период {start:dd.MM.yyyy} — {end:dd.MM.yyyy}")
                ));
            body.Append(title);

            body.Append(new Word.Paragraph(new Word.Run(new Word.Text(""))));

            // Таблица
            var table = new Word.Table();
            table.Append(new Word.TableProperties(
                new Word.TableBorders(
                    new Word.TopBorder { Val = Word.BorderValues.Single },
                    new Word.BottomBorder { Val = Word.BorderValues.Single },
                    new Word.LeftBorder { Val = Word.BorderValues.Single },
                    new Word.RightBorder { Val = Word.BorderValues.Single },
                    new Word.InsideHorizontalBorder { Val = Word.BorderValues.Single },
                    new Word.InsideVerticalBorder { Val = Word.BorderValues.Single }
                )));

            // Заголовок таблицы
            var header = new Word.TableRow();
            header.Append(Cell("Актёр"));
            foreach (var t in eventTypes)
                header.Append(Cell(t));
            table.Append(header);

            // Строки по актёрам
            foreach (var actor in _vm.Actors)
            {
                var row = new Word.TableRow();
                row.Append(Cell(actor.Name));

                foreach (var type in eventTypes)
                {
                    var parts = actor.Participations
                        .Where(p => p.Event != null
                                && p.Event.Type == type
                                && p.Event.Date >= start
                                && p.Event.Date <= end)
                        .ToList();

                    double hours = parts.Sum(p => p.Hours);
                    int participates = parts.Count(p => IsParticipation(p.Event.Type));

                    string text = $"{hours:N1} ч ({participates} уч.)";
                    row.Append(Cell(text));
                }

                table.Append(row);
            }

            body.Append(table);
            main.Document.Append(body);
            main.Document.Save();
        }
    }
}
