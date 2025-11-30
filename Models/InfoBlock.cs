namespace RAG_Code_Base.Models
{
    public class InfoBlock
    {
        public Guid Id { get; set; }
        public Guid FileItemId { get; set; }
        public FileItem FileItem { get; set; }
        public string Content { get; set; }
        public string BlockType { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string? HeaderSection { get; set; }
        public bool IsVectorized { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
