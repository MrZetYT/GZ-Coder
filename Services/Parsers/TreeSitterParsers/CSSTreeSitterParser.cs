using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public class CSSTreeSitterParser : BaseTreeSitterParser
    {
        protected override string GetLanguageName() => "CSS";
        protected override string[] GetFunctionNodeTypes() => Array.Empty<string>();
        protected override string[] GetClassNodeTypes() => Array.Empty<string>();
        protected override string[] GetInterfaceNodeTypes() => Array.Empty<string>();
        protected override string[] GetEnumNodeTypes() => Array.Empty<string>();
        protected override string? ExtractFunctionName(Node node) => null;
        protected override string? ExtractClassName(Node node) => null;
    }
}
