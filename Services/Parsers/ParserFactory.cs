using RAG_Code_Base.Services.Parsers.TreeSitterParsers;

namespace RAG_Code_Base.Services.Parsers
{
    public class ParserFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ParserFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFileParser? GetParser(string fileType)
        {
            return fileType switch
            {
                "Text" => _serviceProvider.GetService<TextFileParser>(),
                "Markdown" => _serviceProvider.GetService<MarkdownParser>(),
                "CSharp" => _serviceProvider.GetService<CSharpParser>(),
                "Python" => _serviceProvider.GetService<PythonTreeSitterParser>(),
                "JavaScript" => _serviceProvider.GetService<JavaScriptTreeSitterParser>(),
                "TypeScript" => _serviceProvider.GetService<TypeScriptTreeSitterParser>(),
                "Java" => _serviceProvider.GetService<JavaTreeSitterParser>(),
                "C++" => _serviceProvider.GetService<CppTreeSitterParser>(),
                "C" => _serviceProvider.GetService<CTreeSitterParser>(),
                "Rust" => _serviceProvider.GetService<RustTreeSitterParser>(),
                "PHP" => _serviceProvider.GetService<PHPTreeSitterParser>(),
                "HTML" => _serviceProvider.GetService<HTMLTreeSitterParser>(),
                "CSS" => _serviceProvider.GetService<CSSTreeSitterParser>(),
                "PDF" => _serviceProvider.GetService<PdfParser>(),
                "DOCX" => _serviceProvider.GetService<DocxParser>(),
                _ => null
            };
        }
    }
}
