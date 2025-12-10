namespace RAG_Code_Base.Models
{
    /// <summary>
    /// Результат поиска похожих блоков
    /// </summary>
    public class SimilarBlock
    {
        public Guid InfoBlockId { get; set; }
        public double Similarity { get; set; }
        public string Content { get; set; }
        public string BlockType { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }

        /// <summary>
        /// Форматированное отображение для UI
        /// </summary>
        public string GetDisplayText()
        {
            var location = $"{FileName}";
            if (!string.IsNullOrEmpty(ClassName))
                location += $" > {ClassName}";
            if (!string.IsNullOrEmpty(MethodName))
                location += $".{MethodName}";
            location += $" (строки {StartLine}-{EndLine})";

            return $"[{Similarity:P1}] {location}";
        }
    }

    /// <summary>
    /// Статистика векторного хранилища
    /// </summary>
    public class VectorStorageStats
    {
        public long TotalVectors { get; set; }
        public int TotalBlocks { get; set; }
        public int VectorizedBlocks { get; set; }
        public double VectorizationProgress { get; set; }

        public override string ToString()
        {
            return $"Векторизовано: {VectorizedBlocks}/{TotalBlocks} ({VectorizationProgress:F1}%)";
        }
    }
}
