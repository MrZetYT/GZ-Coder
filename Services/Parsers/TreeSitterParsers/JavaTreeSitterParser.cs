using RAG_Code_Base.Services.Parsers.TreeSitterParsers;
using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class JavaTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "Java";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[]
            {
                "method_declaration",
                "constructor_declaration"
            };
        }

        protected override string[] GetClassNodeTypes()
        {
            return new[] { "class_declaration" };
        }

        protected override string[] GetInterfaceNodeTypes()
        {
            return new[] { "interface_declaration" };
        }

        protected override string[] GetEnumNodeTypes()
        {
            return new[] { "enum_declaration" };
        }

        protected override string? ExtractClassName(Node node)
        {
            var nameNode = node.GetChildForField("name");
            return nameNode?.Text;
        }

        protected override string? ExtractFunctionName(Node node)
        {
            var nameNode = node.GetChildForField("name");
            return nameNode?.Text;
        }
    }
}