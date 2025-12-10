using RAG_Code_Base.Models;
using RAG_Code_Base.Services.Parsers;
using UglyToad.PdfPig;
using System.Text;

public class PdfParser : IFileParser
{
    public List<InfoBlock> Parse(FileItem fileItem)
    {
        var blocks = new List<InfoBlock>();

        var sb = new StringBuilder();

        using (var document = PdfDocument.Open(fileItem.FilePath))
        {
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text); // весь текст страницы
            }
        }

        string fullText = sb.ToString().Trim();

        if (!string.IsNullOrEmpty(fullText))
        {
            blocks.Add(new InfoBlock
            {
                FileItemId = fileItem.Id,
                Content = fullText,
                BlockType = "Code", // или "Paragraph", если хочешь
                StartLine = 1,
                EndLine = fullText.Count(c => c == '\n') + 1,
                CreatedAt = DateTime.UtcNow
            });
        }

        return blocks;
    }
}
