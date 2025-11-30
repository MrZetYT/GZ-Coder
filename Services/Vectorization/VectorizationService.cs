using LMKit.Model;
using LMKit.Embeddings;
using RAG_Code_Base.Models;
using Microsoft.Extensions.Logging;

namespace RAG_Code_Base.Services.Vectorization
{
    public class VectorizationService
    {
        private readonly LM _model;
        private readonly Embedder _embedder;
        private readonly ILogger<VectorizationService>? _logger;

        public VectorizationService(ILogger<VectorizationService>? logger = null)
        {
            _logger = logger;
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "LM", "bge-m3-Q4_K_M.gguf");
            _model = new LM(modelPath);
            _embedder = new Embedder(_model);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            try
            {
                text = text
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("\\", "/")
                    .Replace("\"", "'");

                var embedding = await _embedder.GetEmbeddingsAsync(text, CancellationToken.None);
                return embedding;
            }
            catch (NullReferenceException ex)
            {
                _logger?.LogError(ex, "LMKit словил NullReference при генерации эмбеддинга. Возможно, невалидный ввод.");
                return Array.Empty<float>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Неизвестная о    шибка при генерации эмбеддинга.");
                return Array.Empty<float>();
            }
        }
    }
}
