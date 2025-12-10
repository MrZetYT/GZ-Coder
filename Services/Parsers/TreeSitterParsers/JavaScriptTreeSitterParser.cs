using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class JavaScriptTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "JavaScript";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[]
            {
                "function_declaration",
                "arrow_function",
                "function",
                "method_definition"
            };
        }

        protected override string[] GetClassNodeTypes()
        {
            return new[] { "class_declaration" };
        }

        protected override string[] GetInterfaceNodeTypes()
        {
            return Array.Empty<string>();
        }

        protected override string[] GetEnumNodeTypes()
        {
            return Array.Empty<string>();
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

            // Для анонимных функций
            return $"<{node.Type}_line_{node.StartPosition.Row + 1}>";
        }
    }
}