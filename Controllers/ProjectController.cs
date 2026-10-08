using Microsoft.AspNetCore.Mvc;
using RAG_Code_Base.Services.ProjectGraph;

namespace RAG_Code_Base.Controllers;

[ApiController]
[Route("api/project")]
public class ProjectController: ControllerBase
{ 
    private readonly ProjectGraphService _projectGraphService;

    public ProjectController(ProjectGraphService projectGraphService)
    {
        _projectGraphService = projectGraphService;
    }
    
    [HttpGet("graph")]
    public async Task<IActionResult> GetGraph()
    {
        var (nodes, edges) = await _projectGraphService.GetGraphAsync();
        return Ok(new {nodes,edges});
    }
}