using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMKit.Model;
using LMKit.TextGeneration;
using Microsoft.Extensions.Logging;
using RAG_Code_Base.Models;
using RAG_Code_Base.Services.Vectorization;
using RAG_Code_Base.Services.VectorStorage;

namespace RAG_Code_Base.Services.Explanation
{
    public class ExplanationService
    {
        private readonly LM _model;
        private readonly MultiTurnConversation _conversation;
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
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "LM", "gemma-3-it-1B-Q4_K_M.gguf");
                _model = new LM(modelPath);
                _conversation = new MultiTurnConversation(_model);
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
                var questionEmbedding = await _vectorizationService.GenerateEmbeddingAsync(question);

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
                var similarBlocks = await _vectorStorageService.SearchSimilarBlocksAsync(
                    questionEmbedding
                );

                

                _logger?.LogInformation("Найдено {Count} релевантных блоков", similarBlocks.Count);

                var contexts = similarBlocks.Select(block =>
                {
                    var location = $"[Файл: {block.FileName}";
                    if (!string.IsNullOrEmpty(block.ClassName))
                        location += $" | Класс: {block.ClassName}";
                    if (!string.IsNullOrEmpty(block.MethodName))
                        location += $" | Метод: {block.MethodName}";
                    location += $" | Строки: {block.StartLine}-{block.EndLine}]";

                    return $"{location}\n\n{block.Content}";
                }).ToList();

                _logger?.LogInformation("Генерация ответа с помощью LLM...");
                var answer = await ExplainInternalAsync(question, contexts, cancellationToken);

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

        private async Task<string> ExplainInternalAsync(
            string question,
            List<string> contexts,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string contextBlock = contexts != null && contexts.Any()
                    ? string.Join("\n\n---\n\n", contexts.Where(c => !string.IsNullOrWhiteSpace(c)))
                    : "Контекст отсутствует.";

                string prompt = $@"
                            Ты — инженер-программист. Объясни код и технический текст простыми словами.
                            Если данных недостаточно — честно скажи, что данных не хватает.

                            Контекст из кодовой базы:
                            {contextBlock}

                            Вопрос пользователя:
                            {question}

                            Ответ:
                            ";

                var response = await _conversation.SubmitAsync(prompt, cancellationToken);

                if (response == null || string.IsNullOrWhiteSpace(response.Completion))
                {
                    _logger?.LogWarning("Модель вернула пустой ответ или null.");
                    return "Модель не смогла сгенерировать ответ.";
                }

                return response.Completion.Trim();
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Генерация ответа была отменена пользователем.");
                return "Генерация отменена.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при генерации объяснения.");
                return "Произошла ошибка при объяснении.";
            }
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