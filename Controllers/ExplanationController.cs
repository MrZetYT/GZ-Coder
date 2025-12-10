using Microsoft.AspNetCore.Mvc;
using RAG_Code_Base.Services.Explanation;
using System.Threading;
using System.Threading.Tasks;

namespace RAG_Code_Base.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExplanationController : ControllerBase
    {
        private readonly ExplanationService _explanationService;
        private readonly ILogger<ExplanationController> _logger;

        public ExplanationController(
            ExplanationService explanationService,
            ILogger<ExplanationController> logger)
        {
            _explanationService = explanationService;
            _logger = logger;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(
            [FromBody] ExplanationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { error = "Вопрос не может быть пустым." });

            _logger.LogInformation("📩 Получен вопрос: '{Question}'", request.Question);

            var response = await _explanationService.ExplainWithSearchAsync(
                request.Question,
                request.TopK ?? 5,
                request.MinSimilarity ?? 0.5,
                cancellationToken
            );

            return Ok(response);
        }

        [HttpGet("ask")]
        public async Task<IActionResult> AskSimple(
            [FromQuery] string q,
            [FromQuery] int topK = 5,
            [FromQuery] double minSimilarity = 0.5,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Параметр 'q' обязателен" });

            var response = await _explanationService.ExplainWithSearchAsync(
                q,
                topK,
                minSimilarity,
                cancellationToken
            );

            return Ok(response);
        }
    }

    public class ExplanationRequest
    {
        public string Question { get; set; } = string.Empty;
        public int? TopK { get; set; } = 5;
        public double? MinSimilarity { get; set; } = 0.5;
    }
}