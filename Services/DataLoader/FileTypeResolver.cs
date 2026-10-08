using System.Text.Json;

namespace RAG_Code_Base.Services.DataLoader
{
    public class FileTypeResolver
    {
        private readonly Dictionary<string, string> _fileTypeMap = new();

        public FileTypeResolver()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Configs", "filetypes.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

                if (data != null)
                {
                    foreach (var category in data.Values)
                    {
                        foreach (var pair in category)
                            _fileTypeMap[pair.Key.ToLower()] = pair.Value;
                    }
                }
            }
        }

        public string GetFileType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return _fileTypeMap.TryGetValue(ext, out var type)
                ? type
                : "Unknown";
        }
    }
}
