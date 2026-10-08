namespace RAG_Code_Base.Models;

public class GraphNode
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
}

public class GraphEdge
{
    public string Source { get; set; }
    public string Target { get; set; }
}