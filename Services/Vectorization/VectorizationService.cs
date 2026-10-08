using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;

namespace RAG_Code_Base.Services.Vectorization
{
    public class VectorizationService : IDisposable
    {
        private readonly LLamaWeights _weights;
        private readonly LLamaEmbedder _embedder;
        private readonly ILogger<VectorizationService>? _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public VectorizationService(ILogger<VectorizationService>? logger = null)
        {
            _logger = logger;

            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "LM", "bge-m3-Q4_K_M.gguf");

            var parameters = new ModelParams(modelPath)
            {
                Embeddings = true,
                ContextSize = 512,
                GpuLayerCount = 0
            };

            _weights = LLamaWeights.LoadFromFile(parameters);
            _embedder = new LLamaEmbedder(_weights, parameters);

            _logger?.LogInformation("VectorizationService инициализирован (LLamaSharp, BGE-M3)");
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                text = text
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("\\", "/")
                    .Replace("\"", "'");

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger?.LogWarning("Передан пустой текст для эмбеддинга.");
                    return Array.Empty<float>();
                }
                
                var embedding = await Task.Run(() => _embedder.GetEmbeddings(text), cancellationToken);

                return embedding[0];
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Генерация эмбеддинга отменена.");
                return Array.Empty<float>();
            }
            catch (Exception ex)
            {

                _logger?.LogError(ex, "Ошибка при генерации эмбеддинга.");

                return Array.Empty<float>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _embedder.Dispose();
            _weights.Dispose();
            _semaphore.Dispose();
        }
    }
}