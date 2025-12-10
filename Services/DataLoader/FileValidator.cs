

using RAG_Code_Base.Database;

namespace RAG_Code_Base.Services.DataLoader
{
    public class FileValidator
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly long _maxFileSizeBytes = 50 * 1024 * 1024;
        private readonly HashSet<string> _allowedExtensions = new()
        {
            ".txt",
            ".rtf",
            ".md",
            ".cs",
            ".py",
            ".java",
            ".js",
            ".ts",
            ".cpp",
            ".hpp",
            ".c",
            ".rs",
            ".php",
            ".html",
            ".css",
            ".pdf",
            ".docx"
        };

        public FileValidator(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public ValidationResult Validate(IFormFile file)
        {
            if(file.Length > _maxFileSizeBytes)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Файл слишком большое. > 50 МБ" };
            }
            
            if(_applicationDbContext.FileItems.Any(n => n.FileName == file.FileName))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Файл с этим именем уже добавлен" };
            }
            
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Этот файл пока не поддерживается. Нет на него парсера" };
            } 

            return new ValidationResult { IsValid = true };
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
