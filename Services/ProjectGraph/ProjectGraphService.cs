using Microsoft.EntityFrameworkCore;
using RAG_Code_Base.Database;
using RAG_Code_Base.Models;

namespace RAG_Code_Base.Services.ProjectGraph;

public class ProjectGraphService
{
    private readonly ApplicationDbContext _applicationDbContext;
    
    public ProjectGraphService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<(List<GraphNode>, List<GraphEdge>)> GetGraphAsync()
    {
        var graphNodes = new List<GraphNode>();
        var graphEdges = new List<GraphEdge>();
        
        var infoBlocks = await _applicationDbContext.InfoBlocks
            .Include(f => f.FileItem)
            .ToListAsync();
        var files = infoBlocks.Select(f => f.FileItem).ToList().Distinct().ToList();
        
        foreach (var file in files)
        {
            graphNodes.Add(new GraphNode
            {
                Id = $"file::{file.FileName}",
                Label = file.FileName,
                Type = "file"
            });
            
            var fileInfoBlocks = infoBlocks.Where(x => x.FileItem == file).ToList();

            var classes = fileInfoBlocks
                .Where(x => x.ClassName != null)
                .Select(x => x.ClassName)
                .Distinct()
                .ToList();

            foreach (var className in classes)
            {
                graphNodes.Add(new GraphNode
                {
                    Id = $"class::{className}",
                    Label = className,
                    Type = "class"
                });
                
                graphEdges.Add(new GraphEdge
                {
                    Source = $"file::{file.FileName}",
                    Target = $"class::{className}",
                });

                var methods = fileInfoBlocks
                    .Where(x => x.ClassName == className && x.MethodName != null)
                    .Select(x => x.MethodName)
                    .Distinct()
                    .ToList();

                foreach (var method in methods)
                {
                    graphNodes.Add(new GraphNode
                    {
                        Id = $"method::{className}::{method}",
                        Label = method,
                        Type = "method"
                    });
                    
                    graphEdges.Add(new GraphEdge
                    {
                        Source = $"class::{className}",
                        Target = $"method::{className}::{method}"
                    });
                }
            }

            var orphanMethods = fileInfoBlocks
                .Where(x => x.ClassName == null && x.MethodName != null)
                .Select(x => x.MethodName)
                .Distinct()
                .ToList();

            foreach (var orphanMethod in orphanMethods)
            {
                graphNodes.Add(new GraphNode
                {
                    Id = $"method::{file.FileName}::{orphanMethod}",
                    Label = orphanMethod,
                    Type = "method"
                });
                
                graphEdges.Add(new GraphEdge
                {
                    Source = $"file::{file.FileName}",
                    Target = $"method::{file.FileName}::{orphanMethod}"
                });
            }
        }

        return (graphNodes,graphEdges);
    }
}