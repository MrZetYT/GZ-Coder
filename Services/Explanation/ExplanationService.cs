using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using RAG_Code_Base.Models;
using RAG_Code_Base.Services.Vectorization;
using RAG_Code_Base.Services.VectorStorage;
using System.Runtime.CompilerServices;

namespace RAG_Code_Base.Services.Explanation
{
    public class ExplanationService : IDisposable
    {
        private readonly LLamaWeights _weights;
        private readonly ModelParams _modelParams;
        private readonly ILogger<ExplanationService>? _logger;
        private readonly VectorizationService _vectorizationService;
        private readonly VectorStorageService _vectorStorageService;

        public ExplanationService(
            VectorizationService vectorizationService,
            VectorStorageService vectorStorageService,
            ILogger<ExplanationService>? logger = null)
        {
            _logger = logger;
            _vectorizationService = vectorizationService;
            _vectorStorageService = vectorStorageService;

            try
            {
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "LM", "Qwen2.5-Coder-3B-Instruct-Q4_K_M.gguf");

                _modelParams = new ModelParams(modelPath)
                {
                    ContextSize = 4096,
                    GpuLayerCount = 0
                };

                _weights = LLamaWeights.LoadFromFile(_modelParams);

                _logger?.LogInformation("ExplanationService инициализирован (LLamaSharp)");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при инициализации модели LLM.");
                throw;
            }
        }

        public async Task<ExplanationResponse> ExplainWithSearchAsync(
            string question,
            int topK = 5,
            double minSimilarity = 0.5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Получен вопрос: '{Question}'", question);

                _logger?.LogInformation("Генерация эмбеддинга для вопроса...");
                var questionEmbedding = await _vectorizationService.GenerateEmbeddingAsync(question, cancellationToken);

                if (questionEmbedding == null || questionEmbedding.Length == 0)
                {
                    _logger?.LogWarning("Не удалось создать эмбеддинг для вопроса");
                    return new ExplanationResponse
                    {
                        Question = question,
                        Answer = "Не удалось обработать ваш вопрос. Попробуйте переформулировать.",
                        FoundBlocks = new List<SimilarBlock>()
                    };
                }

                _logger?.LogInformation("Поиск релевантных блоков в векторной базе...");
                var similarBlocks = (await _vectorStorageService.SearchSimilarBlocksAsync(questionEmbedding))
                    .Take(3)
                    .ToList();

                _logger?.LogInformation("Найдено {Count} релевантных блоков", similarBlocks.Count);

                var contexts = similarBlocks.Select(block =>
                {
                    var location = $"[Файл: {block.FileName}";
                    if (!string.IsNullOrEmpty(block.ClassName))
                        location += $" | Класс: {block.ClassName}";
                    if (!string.IsNullOrEmpty(block.MethodName))
                        location += $" | Метод: {block.MethodName}";
                    location += $" | Строки: {block.StartLine}-{block.EndLine}]";

                    var content = block.Content.Length > 300
                        ? block.Content[..300] + "..."
                        : block.Content;

                    return $"{location}\n\n{content}";
                }).ToList();

                _logger?.LogInformation("Генерация ответа с помощью LLM...");

                var sb = new StringBuilder();
                await foreach (var token in ExplainInternalAsync(question, contexts, cancellationToken))
                    sb.Append(token);
                var answer = sb.ToString().Trim();

                _logger?.LogInformation("Ответ успешно сгенерирован");

                return new ExplanationResponse
                {
                    Question = question,
                    Answer = answer,
                    FoundBlocks = similarBlocks
                };
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Генерация ответа была отменена");
                return new ExplanationResponse
                {
                    Question = question,
                    Answer = "Генерация ответа была отменена.",
                    FoundBlocks = new List<SimilarBlock>()
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при генерации объяснения");
                return new ExplanationResponse
                {
                    Question = question,
                    Answer = $"Произошла ошибка при обработке вопроса: {ex.Message}",
                    FoundBlocks = new List<SimilarBlock>()
                };
            }
        }

        public async IAsyncEnumerable<string> ExplainStreamAsync(
            string question,
            int topK = 5,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var questionEmbedding = await _vectorizationService.GenerateEmbeddingAsync(question, cancellationToken);

            var similarBlocks = (await _vectorStorageService.SearchSimilarBlocksAsync(questionEmbedding))
                .Take(topK)
                .ToList();

            var contexts = similarBlocks.Select(block =>
            {
                var location = $"[Файл: {block.FileName} | Строки: {block.StartLine}-{block.EndLine}]";
                var content = block.Content.Length > 300 ? block.Content[..300] + "..." : block.Content;
                return $"{location}\n\n{content}";
            }).ToList();

            await foreach (var token in ExplainInternalAsync(question, contexts, cancellationToken))
            {
                yield return token;
            }
        }

        private async IAsyncEnumerable<string> ExplainInternalAsync(
            string question,
            List<string> contexts,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string contextBlock = contexts != null && contexts.Any()
                ? string.Join("\n\n---\n\n", contexts.Where(c => !string.IsNullOrWhiteSpace(c)))
                : "Контекст отсутствует.";

            string prompt = $"""
                             Ты — инженер-программист. Объясни код и технический текст простыми словами.

								ИНСТРУКЦИИ ПО ФОРМАТИРОВАНИЮ:
								- Используй **markdown** для форматирования
								- Код выделяй в блоки: ```csharp ... ```
								- Важные термины выделяй **жирным**
								- Списки оформляй через "- " или "1. "
								- Разбивай ответ на абзацы для читаемости

							 Если данных недостаточно — честно скажи об этом.

							 Контекст из кодовой базы:
							 {contextBlock}

							 Вопрос пользователя:
							 {question}

							 Ответ:
                             """;

            var inferenceParams = new InferenceParams
            {
                MaxTokens = 512,
                AntiPrompts = new List<string> { "Вопрос пользователя:", "Контекст из кодовой базы:" }
            };

            var executor = new StatelessExecutor(_weights, _modelParams);

            await foreach (var token in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                yield return token;
            }
        }

        public void Dispose()
        {
            _weights.Dispose();
        }
    }

    public class ExplanationResponse
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public List<SimilarBlock> FoundBlocks { get; set; }

        public string GetSourcesSummary()
        {
            if (FoundBlocks == null || FoundBlocks.Count == 0)
                return "Источники не найдены";

            return string.Join("\n", FoundBlocks.Select((block, i) =>
                $"{i + 1}. {block.GetDisplayText()}"
            ));
        }
    }
}