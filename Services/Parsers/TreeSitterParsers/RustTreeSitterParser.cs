using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class RustTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "Rust";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[] { "function_item" };
        }

        protected override string[] GetClassNodeTypes()
        {
            return new[] { "struct_item", "impl_item" };
        }

        protected override string[] GetInterfaceNodeTypes()
        {
            return new[] { "trait_item" };
        }

        protected override string[] GetEnumNodeTypes()
        {
            return new[] { "enum_item" };
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
