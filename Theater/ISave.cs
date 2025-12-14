using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Theater
{
    public interface ISave
    {
        void Save(string filepath, SaveTable table);
    }

    public class SaveTable
    {
        public string Title { get; set; } = "";
        public List<string> Columns { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }
    public class WordSaveService : ISave
    {
        public void Save(string filepath, SaveTable table)
        {
            using (var doc = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document))
            {
                var main = doc.AddMainDocumentPart();
                var body = new Word.Body();

                // Заголовок
                if (!string.IsNullOrEmpty(table.Title))
                {
                    var titleRun = new Word.Run(
                        new Word.RunProperties(new Word.Bold()),
                        new Word.Text(table.Title)
                    );
                    body.Append(new Word.Paragraph(titleRun));
                }

                // Пустая строка
                body.Append(new Word.Paragraph(new Word.Run(new Word.Text(""))));

                // Таблица
                var wordTable = new Word.Table();
                var borders = new Word.TableBorders(
                    new Word.TopBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.BottomBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.LeftBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.RightBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.InsideHorizontalBorder { Val = Word.BorderValues.Single, Size = 4 },
                    new Word.InsideVerticalBorder { Val = Word.BorderValues.Single, Size = 4 }
                );

                wordTable.Append(new Word.TableProperties(borders));

                // Заголовок таблицы
                var header = new Word.TableRow();
                foreach (var col in table.Columns)
                    header.Append(MakeCell(col, true));
                wordTable.Append(header);

                // Строки данных
                foreach (var row in table.Rows)
                {
                    var wordRow = new Word.TableRow();
                    foreach (var cell in row)
                        wordRow.Append(MakeCell(cell));
                    wordTable.Append(wordRow);
                }

                body.Append(wordTable);
                main.Document = new Word.Document(body);
            }
        }

        private Word.TableCell MakeCell(string text, bool bold = false)
        {
            var run = new Word.Run();

            if (bold)
                run.Append(new Word.RunProperties(new Word.Bold()));

            run.Append(new Word.Text(text ?? ""));

            return new Word.TableCell(new Word.Paragraph(run));
        }

    }
}
