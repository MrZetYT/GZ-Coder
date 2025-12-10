using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    //TODO: добавить виденье глобальных переменных и комментариев отдельных
    public class PythonTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName()
        {
            return "Python";
        }

        protected override string[] GetFunctionNodeTypes()
        {
            return new[] { "function_definition" };
        }

        protected override string[] GetClassNodeTypes()
        {
            return new[] { "class_definition" };
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
            return nameNode?.Text;
        }
    }
}