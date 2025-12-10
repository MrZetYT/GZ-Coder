using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class PHPTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "PHP";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[] { "function_declaration", "method_declaration" };
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
            foreach (var child in node.NamedChildren)
            {
                if (child.Type == "name" || child.Type == "identifier")
                    return child.Text;
            }

            return null;
        }

        protected override string? ExtractFunctionName(Node node)
        {
            foreach (var child in node.NamedChildren)
            {
                if (child.Type == "name" || child.Type == "identifier")
                    return child.Text;
            }

            return null;
        }
    }
}
