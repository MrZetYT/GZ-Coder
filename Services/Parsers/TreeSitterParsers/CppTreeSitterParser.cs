using RAG_Code_Base.Services.Parsers.TreeSitterParsers;
using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class CppTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "Cpp";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[]
            {
                "function_definition",
                "function_declarator"
            };
        }

        protected override string[] GetClassNodeTypes()
        {
            return new[]
            {
                "class_specifier",
                "struct_specifier"
            };
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
                {
                    return child.Text;
                }
            }
            return null;
        }

        protected override string? ExtractFunctionName(Node node)
        {
            var declarator = node.GetChildForField("declarator");
            if (declarator != null)
            {
                foreach (var child in declarator.NamedChildren)
                {
                    if (child.Type == "identifier" || child.Type == "field_identifier")
                    {
                        return child.Text;
                    }
                }
            }
            return null;
        }
    }
}