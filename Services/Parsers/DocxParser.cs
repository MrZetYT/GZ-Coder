using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using RAG_Code_Base.Models;
using RAG_Code_Base.Services.Parsers;

public class DocxParser : IFileParser
{
    public List<InfoBlock> Parse(FileItem fileItem)
    {
        var blocks = new List<InfoBlock>();
        int currentLine = 1;

        using (var doc = WordprocessingDocument.Open(fileItem.FilePath, false))
        {
            var body = doc.MainDocumentPart.Document.Body;

            List<string> buffer = new List<string>();
            string currentBlockType = null;

            foreach (var element in body.Elements())
            {
                if (element is Paragraph paragraph)
                {
                    string text = paragraph.InnerText?.Trim();
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    string blockType = GetParagraphType(paragraph, text);

                    if (currentBlockType != null && currentBlockType != blockType)
                    {
                        AddBufferAsBlock(blocks, buffer, currentBlockType, fileItem.Id, ref currentLine);
                        buffer.Clear();
                    }

                    buffer.Add(text);
                    currentBlockType = blockType;
                }
            }

            if (buffer.Count > 0)
                AddBufferAsBlock(blocks, buffer, currentBlockType, fileItem.Id, ref currentLine);
        }

        return blocks;
    }

    private string GetParagraphType(Paragraph paragraph, string text)
    {
        var pStyle = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        if (!string.IsNullOrEmpty(pStyle))
        {
            if (pStyle.StartsWith("Heading")) return "Header";
            if (pStyle.StartsWith("Code") || pStyle.StartsWith("Preformatted")) return "Code";
        }

        if (paragraph.ParagraphProperties?.NumberingProperties != null)
            return "List";

        if (text.StartsWith("- ") || text.StartsWith("* ") || System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.\s"))
            return "List";

        return "Paragraph";
    }

    private void AddBufferAsBlock(List<InfoBlock> blocks, List<string> buffer, string blockType, Guid fileItemId, ref int currentLine)
    {
        if (buffer.Count == 0) return;

        blocks.Add(new InfoBlock
        {
            FileItemId = fileItemId,
            Content = string.Join("\n", buffer),
            BlockType = blockType,
            StartLine = currentLine,
            EndLine = currentLine + buffer.Count - 1,
            CreatedAt = DateTime.UtcNow
        });

        currentLine += buffer.Count + 1;
    }
}
