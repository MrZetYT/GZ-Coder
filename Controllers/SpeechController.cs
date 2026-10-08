using Microsoft.AspNetCore.Mvc;
using RAG_Code_Base.Services.Speech;
using RAG_Code_Base.Models;

namespace RAG_Code_Base.Controllers;

[ApiController]
[Route("api/speech")]
public class SpeechController: ControllerBase
{
    private readonly SpeechService _speechService;

    public SpeechController(SpeechService speechService)
    {
        _speechService = speechService;
    }

    [HttpPost("recognize")]
    public async Task<IActionResult> PostAudio(IFormFile audio)
    {
        var ext = Path.GetExtension(audio.FileName).ToLower();
        if(ext != ".wav")
            return BadRequest(new ApiError("unsupported_format","Поддерживается только WAV формат"));
        
        if (audio == null || audio.Length == 0)
            return BadRequest(new ApiError("empty_file","Файл пуст"));
        
        var result = await _speechService.RecognizeAsync(audio);
        return Ok(new { transcript = result });
    }
}