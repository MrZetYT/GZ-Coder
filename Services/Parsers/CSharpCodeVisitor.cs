using Hangfire.PostgreSql.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RAG_Code_Base.Models;

namespace RAG_Code_Base.Services.Parsers
{
    public class CSharpCodeVisitor : CSharpSyntaxWalker
    {
        private readonly Guid _fileItemId;
        private readonly List<InfoBlock> _blocks = new();

        public CSharpCodeVisitor(Guid fileItemId)
        {
            _fileItemId = fileItemId;
        }

        public List<InfoBlock> GetBlocks() => _blocks;

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            var parent = node.Parent;

            var infoBlock = new InfoBlock()
            {
                FileItemId = _fileItemId,
                Content = node.ToFullString(),
                BlockType = "Method",
                MethodName = node.Identifier.Text,
                ClassName = GetClassName(node),
                StartLine = lineSpan.StartLinePosition.Line + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                CreatedAt = DateTime.UtcNow
            };

            _blocks.Add(infoBlock);

            base.VisitMethodDeclaration(node);
        }

        public string? GetClassName(SyntaxNode node)
        {
            var classDecl = node.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classDecl != null)
                return classDecl.Identifier.Text;

            var interfaceDecl = node.Ancestors()
                .OfType<InterfaceDeclarationSyntax>()
                .FirstOrDefault();

            return interfaceDecl?.Identifier.Text;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            var infoBlock = new InfoBlock()
            {
                FileItemId = _fileItemId,
                Content = node.ToFullString(),
                BlockType = "Class",
                ClassName = node.Identifier.Text,
                StartLine = lineSpan.StartLinePosition.Line + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                CreatedAt = DateTime.UtcNow
            };

            _blocks.Add(infoBlock);

            base.VisitClassDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            var infoBlock = new InfoBlock()
            {
                FileItemId = _fileItemId,
                Content = node.ToFullString(),
                BlockType = "Interface",
                ClassName = node.Identifier.Text,
                StartLine = lineSpan.StartLinePosition.Line + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                CreatedAt = DateTime.UtcNow
            };

            _blocks.Add(infoBlock);

            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            var infoBlock = new InfoBlock()
            {
                FileItemId = _fileItemId,
                Content = node.ToFullString(),
                BlockType = "Enum",
                ClassName = node.Identifier.Text,
                StartLine = lineSpan.StartLinePosition.Line + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                CreatedAt = DateTime.UtcNow
            };

            _blocks.Add(infoBlock);

            base.VisitEnumDeclaration(node);
        }
    }
}
