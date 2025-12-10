using TreeSitter;
namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class CTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "C";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[] { "function_definition" };
        }

        protected override string[] GetClassNodeTypes()
        {
            return new[] { "struct_specifier" };
        }

        protected override string[] GetInterfaceNodeTypes()
        {
            return Array.Empty<string>();
        }

        protected override string[] GetEnumNodeTypes()
        {
            return new[] { "enum_specifier" };
        }

        protected override string? ExtractClassName(Node node)
        {
            foreach (var child in node.NamedChildren)
            {
                if (child.Type == "type_identifier")
                    return child.Text;
            }

            return null;
        }

        protected override string? ExtractFunctionName(Node node)
        {
            foreach (var child in node.NamedChildren)
            {
                if (child.Type == "identifier")
                    return child.Text;
            }

            return null;
        }
    }
}
