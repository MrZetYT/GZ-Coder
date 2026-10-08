using Microsoft.AspNetCore.Mvc;
using RAG_Code_Base.Services.Explanation;
using RAG_Code_Base.Models;

namespace RAG_Code_Base.Controllers
{
    [ApiController]
    [Route("api")]
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

        [HttpPost("query")]
        public async Task<IActionResult> Ask(
            [FromBody] ExplanationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new ApiError("null_question","Вопрос не может быть пустым"));

            _logger.LogInformation("📩 Получен вопрос: '{Question}'", request.Question);

            var response = await _explanationService.ExplainWithSearchAsync(
                request.Question,
                request.TopK ?? 5,
                request.MinSimilarity ?? 0.5,
                cancellationToken
            );

            return Ok(response);
        }
        
        [HttpPost("query/stream")]
        public async Task StreamAnswer(
            [FromBody] ExplanationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("data: {\"error\": \"Вопрос не может быть пустым\"}\n\n", cancellationToken);
                return;
            }
            
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            await foreach (var token in _explanationService.ExplainStreamAsync(
                               request.Question,
                               request.TopK ?? 5,
                               cancellationToken))
            {
                await Response.WriteAsync($"data: {token}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    public class ExplanationRequest
    {
        public string Question { get; set; } = string.Empty;
        public int? TopK { get; set; } = 5;
        public double? MinSimilarity { get; set; } = 0.5;
    }
}