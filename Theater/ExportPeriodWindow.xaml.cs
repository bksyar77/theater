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
                .Where(ev => ev.Date != null && ev.Date >= start && ev.Date <= end)
                .Select(ev => ev.Type ?? "Без типа")
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            using (var doc = WordprocessingDocument.Create(filename, WordprocessingDocumentType.Document))
            {
                var main = doc.AddMainDocumentPart();

                var body = new Word.Body();

                // Заголовок
                var titleRun = new Word.Run();
                titleRun.Append(new Word.RunProperties(new Word.Bold()));
                titleRun.Append(new Word.Text($"Статистика за период {start:dd.MM.yyyy} — {end:dd.MM.yyyy}"));

                var titleParagraph = new Word.Paragraph(titleRun);
                body.Append(titleParagraph);

                // Пустая строка
                body.Append(new Word.Paragraph(new Word.Run(new Word.Text(string.Empty))));

                // Если нет данных — выводим сообщение и выходим
                if (_vm.Actors.Count == 0 || eventTypes.Count == 0)
                {
                    body.Append(new Word.Paragraph(new Word.Run(new Word.Text("Нет данных за указанный период."))));
                    main.Document = new Word.Document(body);
                    return;
                }

                // Таблица
                var table = new Word.Table();

                var borders = new Word.TableBorders(
                    new Word.TopBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.BottomBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.LeftBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.RightBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.InsideHorizontalBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.InsideVerticalBorder { Val = Word.BorderValues.Single, Size = 4 }
                );

                table.Append(new Word.TableProperties(borders));

                // Заголовок таблицы
                var header = new Word.TableRow();
                header.Append(MakeCell("Актёр", true));
                foreach (var type in eventTypes)
                    header.Append(MakeCell(type, true));
                table.Append(header);

                // Строки по актёрам
                foreach (var actor in _vm.Actors)
                {
                    var row = new Word.TableRow();
                    row.Append(MakeCell(actor.Name));

                    foreach (var type in eventTypes)
                    {
                        var parts = actor.Participations
                            .Where(p => p.Event != null
                                     && (p.Event.Type ?? "Без типа") == type
                                     && p.Event.Date >= start
                                     && p.Event.Date <= end)
                            .ToList();

                        double hours = parts.Sum(p => p.Hours);

                        int participates = parts.Count(p =>
                            p.Event.Type == "Спектакль" ||
                            p.Event.Type == "Спектакль на выезде" ||
                            p.Event.Type == "Спектакль на стационаре");

                        string txt = $"{hours:N1} ч ({participates} уч.)";

                        row.Append(MakeCell(txt));
                    }

                    table.Append(row);
                }

                body.Append(table);

                main.Document = new Word.Document(body);
            }
        }


        private Word.TableCell MakeCell(string text, bool bold = false)
        {
            var run = new Word.Run();

            if (bold)
                run.Append(new Word.RunProperties(new Word.Bold()));

            run.Append(new Word.Text(text ?? string.Empty));

            var paragraph = new Word.Paragraph(run);
            return new Word.TableCell(paragraph);
        }

    }
}
