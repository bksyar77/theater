using DocumentFormat.OpenXml.Packaging;
using Word = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace Theater.Services
{
    public class WordSaveService : ISave
    {
        public void Save(string filepath, SaveTable table)
        {
            using var doc = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document);
            var main = doc.AddMainDocumentPart();
            var body = new Word.Body();

            if (!string.IsNullOrEmpty(table.Title))
            {
                var titleRun = new Word.Run(
                    new Word.RunProperties(new Word.Bold()),
                    new Word.Text(table.Title)
                );
                body.Append(new Word.Paragraph(titleRun));
            }

            body.Append(new Word.Paragraph(new Word.Run(new Word.Text(""))));

            var wordTable = new Word.Table();
            wordTable.Append(new Word.TableProperties(new Word.TableBorders(
                new Word.TopBorder { Val = Word.BorderValues.Single, Size = 4 },
                new Word.BottomBorder { Val = Word.BorderValues.Single, Size = 4 },
                new Word.LeftBorder { Val = Word.BorderValues.Single, Size = 4 },
                new Word.RightBorder { Val = Word.BorderValues.Single, Size = 4 },
                new Word.InsideHorizontalBorder { Val = Word.BorderValues.Single, Size = 4 },
                new Word.InsideVerticalBorder { Val = Word.BorderValues.Single, Size = 4 }
            )));

            var header = new Word.TableRow();
            foreach (var col in table.Columns)
                header.Append(MakeCell(col, true));
            wordTable.Append(header);

            foreach (var row in table.Rows)
            {
                var r = new Word.TableRow();
                foreach (var cell in row)
                    r.Append(MakeCell(cell));
                wordTable.Append(r);
            }

            body.append(wordTable)
            main.Document = new Word.Document(body);
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
