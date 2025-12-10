using Microsoft.AspNetCore.Mvc;
using RAG_Code_Base.Database;
using RAG_Code_Base.Models;
using RAG_Code_Base.Services.Vectorization;
using RAG_Code_Base.Services.VectorStorage;

namespace RAG_Code_Base.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly VectorizationService _vectorizationService;
        private readonly VectorStorageService _vectorStorageService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            VectorizationService vectorizationService,
            VectorStorageService vectorStorageService,
            ILogger<SearchController> logger)
        {
            _vectorizationService = vectorizationService;
            _vectorStorageService = vectorStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Поиск похожих блоков кода по текстовому запросу
        /// </summary>
        [HttpPost("query")]
        public async Task<IActionResult> SearchByQuery([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest("Запрос не может быть пустым");
                }

                _logger.LogInformation("🔍 Поиск по запросу: '{Query}'", request.Query);

                // 1. Векторизуем запрос пользователя
                var queryEmbedding = await _vectorizationService.GenerateEmbeddingAsync(request.Query);

                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    return BadRequest("Не удалось создать вектор для запроса");
                }

                // 2. Ищем похожие блоки в Qdrant
                var similarBlocks = await _vectorStorageService.SearchSimilarBlocksAsync(
                    queryEmbedding
                );

                return Ok(new SearchResponse
                {
                    Query = request.Query,
                    Results = similarBlocks,
                    Count = similarBlocks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при поиске");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить статистику векторной базы
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromServices] ApplicationDbContext dbContext)
        {
            try
            {
                var stats = await _vectorStorageService.GetStatsAsync(dbContext);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении статистики");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request/Response модели
    public class SearchRequest
    {
        public string Query { get; set; }
        public int? TopK { get; set; } = 5;
        public double? MinSimilarity { get; set; } = 0.3;
    }

    public class SearchResponse
    {
        public string Query { get; set; }
        public List<SimilarBlock> Results { get; set; }
        public int Count { get; set; }
    }
}