using Microsoft.AspNetCore.Mvc;
using RAG_Code_Base.Services.DataLoader;

namespace RAG_Code_Base.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileLoaderController : ControllerBase
    {
        private readonly FileLoaderService _fileLoaderService;
        private readonly FileValidator _fileValidator;

        public FileLoaderController(FileLoaderService fileLoaderService, FileValidator fileValidator)
        {
            _fileLoaderService = fileLoaderService;
            _fileValidator = fileValidator;
        }

        [HttpPost("upload")]
        public IActionResult UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("Файлы не выбраны.");

            foreach (var file in files)
            {
                var validationResult = _fileValidator.Validate(file);
                if (!validationResult.IsValid)
                    return BadRequest($"Файл {file.FileName} — ошибка: {validationResult.ErrorMessage}");
            }

            var savedFiles = _fileLoaderService.SaveFiles(files);
            return Ok(savedFiles);
        }


        [HttpGet("")]
        public IActionResult GetFilesList()
        {
            var gotFiles = _fileLoaderService.GetAllFiles();
            return Ok(gotFiles);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFileAsync(Guid id)
        {
            var deletedFile = await _fileLoaderService.DeleteFileAsync(id);
            if (!deletedFile) return NotFound("Файл не найден.");
            return NoContent();
        }

        [HttpDelete("DeleteAll")]
        public async Task<IActionResult> DeleteAllFilesAsync()
        {
            var isDeleted = await _fileLoaderService.DeleteAllFilesAsync();
            if (!isDeleted) return NotFound("Файлов нет");
            return NoContent();
        }
    }
}
