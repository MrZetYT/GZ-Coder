using RAG_Code_Base.Services.Parsers.TreeSitterParsers;
using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class TypeScriptTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "TypeScript";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[]
            {
                "function_declaration",
                "arrow_function",
                "function",
                "method_definition",
                "function_signature"
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
            if (nameNode != null)
            {
                return nameNode.Text;
            }

            return $"<{node.Type}_line_{node.StartPosition.Row + 1}>";
        }
    }
}