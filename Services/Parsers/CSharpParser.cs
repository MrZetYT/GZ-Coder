using Microsoft.CodeAnalysis.CSharp;
using RAG_Code_Base.Models;

namespace RAG_Code_Base.Services.Parsers
{
    public class CSharpParser : IFileParser
    {
        public List<InfoBlock> Parse(FileItem fileItem)
        {
            var content = File.ReadAllText(fileItem.FilePath);

            var tree = CSharpSyntaxTree.ParseText(content);

            var root = tree.GetRoot();

            var visitor = new CSharpCodeVisitor(fileItem.Id);

            visitor.Visit(root);

            return visitor.GetBlocks();
        }
    }
}
