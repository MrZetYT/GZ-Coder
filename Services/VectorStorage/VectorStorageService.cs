using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using RAG_Code_Base.Database;
using RAG_Code_Base.Models;

namespace RAG_Code_Base.Services.VectorStorage
{
    public class VectorStorageService
    {
        private readonly ILogger<VectorStorageService> _logger;
        private readonly QdrantClient _qdrantClient;
        private const string CollectionName = "code_embeddings";
        private const int VectorSize = 1024;

        public VectorStorageService(
            ILogger<VectorStorageService> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            // Получаем настройки Qdrant из appsettings.json
            var qdrantHost = configuration.GetValue<string>("Qdrant:Host") ?? "localhost";
            var qdrantPort = configuration.GetValue<int>("Qdrant:Port", 6334);

            // Создаём клиент для работы с Qdrant
            _qdrantClient = new QdrantClient(qdrantHost, qdrantPort);

            _logger.LogInformation("🔌 Подключение к Qdrant: {Host}:{Port}", qdrantHost, qdrantPort);

            // Инициализируем коллекцию при запуске
            InitializeCollectionAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Создаёт коллекцию в Qdrant, если её ещё нет
        /// </summary>
        private async Task InitializeCollectionAsync()
        {
            try
            {
                // Пробуем получить информацию о коллекции
                // Если коллекция существует - метод вернёт её информацию
                // Если нет - выбросит исключение
                try
                {
                    await _qdrantClient.GetCollectionInfoAsync(CollectionName);
                    _logger.LogInformation("✅ Коллекция {CollectionName} уже существует", CollectionName);
                }
                catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    // Коллекции нет - создаём её
                    _logger.LogInformation("📦 Создание коллекции {CollectionName}...", CollectionName);

                    await _qdrantClient.CreateCollectionAsync(
                        collectionName: CollectionName,
                        vectorsConfig: new VectorParams
                        {
                            Size = VectorSize,
                            Distance = Distance.Cosine
                        }
                    );

                    _logger.LogInformation("✅ Коллекция {CollectionName} создана", CollectionName);
                }
            }
            catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
            {
                // Коллекция уже существует (race condition) - это нормально
                _logger.LogInformation("✅ Коллекция {CollectionName} уже существует", CollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при инициализации коллекции Qdrant");
                throw;
            }
        }

        /// <summary>
        /// Сохраняет эмбеддинг для блока информации в Qdrant
        /// Вызывается из FileLoaderService.VectorizeBlockInBackgroundAsync
        /// </summary>
        public async Task SaveEmbeddingAsync(Guid blockId, float[] embedding, ApplicationDbContext dbContext)
        {
            try
            {
                _logger.LogInformation("💾 Сохранение эмбеддинга для блока {BlockId}, размер: {Size}",
                    blockId, embedding?.Length ?? 0);

                if (embedding == null || embedding.Length == 0)
                {
                    _logger.LogWarning("⚠️ Попытка сохранить пустой эмбеддинг для блока {BlockId}", blockId);
                    return;
                }

                // Получаем информацию о блоке из PostgreSQL
                var infoBlock = await dbContext.InfoBlocks
                    .Include(b => b.FileItem)
                    .FirstOrDefaultAsync(b => b.Id == blockId);

                if (infoBlock == null)
                {
                    _logger.LogWarning("⚠️ Блок {BlockId} не найден в базе данных", blockId);
                    return;
                }

                // Подготавливаем метаданные (payload) для сохранения вместе с вектором
                var payload = new Dictionary<string, Value>
                {
                    ["info_block_id"] = blockId.ToString(),
                    ["content"] = infoBlock.Content,
                    ["block_type"] = infoBlock.BlockType,
                    ["start_line"] = infoBlock.StartLine,
                    ["end_line"] = infoBlock.EndLine,
                    ["file_name"] = infoBlock.FileItem.FileName,
                    ["file_type"] = infoBlock.FileItem.FileType,
                    ["file_item_id"] = infoBlock.FileItemId.ToString(),
                    ["created_at"] = DateTime.UtcNow.ToString("O")
                };

                // Добавляем опциональные поля, если они есть
                if (!string.IsNullOrEmpty(infoBlock.ClassName))
                    payload["class_name"] = infoBlock.ClassName;

                if (!string.IsNullOrEmpty(infoBlock.MethodName))
                    payload["method_name"] = infoBlock.MethodName;

                if (!string.IsNullOrEmpty(infoBlock.HeaderSection))
                    payload["header_section"] = infoBlock.HeaderSection;

                // Создаём точку (point) для сохранения в Qdrant
                var pointStruct = new PointStruct
                {
                    Id = new PointId { Uuid = blockId.ToString() },
                    Vectors = embedding,
                    Payload = { payload }
                };

                // Сохраняем в Qdrant (Upsert = создать или обновить)
                await _qdrantClient.UpsertAsync(
                    collectionName: CollectionName,
                    points: new[] { pointStruct }
                );

                _logger.LogInformation("✅ Эмбеддинг успешно сохранён в Qdrant для блока {BlockId}", blockId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при сохранении эмбеддинга для блока {BlockId}", blockId);
                throw;
            }
        }

        /// <summary>
        /// Ищет наиболее похожие блоки по векторному представлению запроса
        /// </summary>
        public async Task<List<SimilarBlock>> SearchSimilarBlocksAsync(
            float[] queryEmbedding,
            int topK = 5,
            double minSimilarity = 0.5)
        {
            try
            {
                _logger.LogInformation("🔍 Поиск похожих блоков: topK={TopK}, minSimilarity={MinSim}",
                    topK, minSimilarity);

                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    _logger.LogWarning("⚠️ Пустой вектор запроса");
                    return new List<SimilarBlock>();
                }

                // Выполняем поиск в Qdrant
                var searchResults = await _qdrantClient.SearchAsync(
                    collectionName: CollectionName,
                    vector: queryEmbedding,
                    limit: (ulong)topK,
                    scoreThreshold: (float)minSimilarity
                );

                var similarBlocks = new List<SimilarBlock>();

                // Преобразуем результаты Qdrant в нашу модель
                foreach (var result in searchResults)
                {
                    var payload = result.Payload;

                    similarBlocks.Add(new SimilarBlock
                    {
                        InfoBlockId = Guid.Parse(payload["info_block_id"].StringValue),
                        Similarity = result.Score,
                        Content = payload["content"].StringValue,
                        BlockType = payload["block_type"].StringValue,
                        ClassName = payload.ContainsKey("class_name")
                            ? payload["class_name"].StringValue
                            : null,
                        MethodName = payload.ContainsKey("method_name")
                            ? payload["method_name"].StringValue
                            : null,
                        StartLine = (int)payload["start_line"].IntegerValue,
                        EndLine = (int)payload["end_line"].IntegerValue,
                        FileName = payload["file_name"].StringValue,
                        FileType = payload["file_type"].StringValue
                    });
                }

                _logger.LogInformation("✅ Найдено {Count} похожих блоков", similarBlocks.Count);
                return similarBlocks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при поиске похожих блоков");
                throw;
            }
        }

        /// <summary>
        /// Получает статистику по векторной базе
        /// </summary>
        public async Task<VectorStorageStats> GetStatsAsync(ApplicationDbContext dbContext)
        {
            try
            {
                // Статистика из Qdrant
                var collectionInfo = await _qdrantClient.GetCollectionInfoAsync(CollectionName);
                var totalVectors = (long)collectionInfo.VectorsCount;

                // Статистика из PostgreSQL
                var totalBlocks = await dbContext.InfoBlocks.CountAsync();
                var vectorizedBlocks = await dbContext.InfoBlocks.CountAsync(b => b.IsVectorized);

                return new VectorStorageStats
                {
                    TotalVectors = totalVectors,
                    TotalBlocks = totalBlocks,
                    VectorizedBlocks = vectorizedBlocks,
                    VectorizationProgress = totalBlocks > 0
                        ? Math.Round((double)vectorizedBlocks / totalBlocks * 100, 2)
                        : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении статистики");
                return new VectorStorageStats();
            }
        }

        /// <summary>
        /// Удаляет эмбеддинг для блока из Qdrant
        /// </summary>
        public async Task DeleteEmbeddingAsync(Guid blockId)
        {
            try
            {
                await _qdrantClient.DeleteAsync(
                    collectionName: CollectionName,
                    ids: new[] { new PointId { Uuid = blockId.ToString() } }
                );

                _logger.LogInformation("Эмбеддинг для блока {BlockId} удалён из Qdrant", blockId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении эмбеддинга для блока {BlockId}", blockId);
                throw;
            }
        }

        /// <summary>
        /// Удаляет все векторы, связанные с файлом
        /// </summary>
        public async Task DeleteFileEmbeddingsAsync(Guid fileItemId)
        {
            try
            {
                _logger.LogInformation("🗑️ Удаление всех векторов для файла {FileId}", fileItemId);

                // Удаляем по фильтру
                await _qdrantClient.DeleteAsync(
                    collectionName: CollectionName,
                    filter: new Filter
                    {
                        Must =
                        {
                            new Condition
                            {
                                Field = new FieldCondition
                                {
                                    Key = "file_item_id",
                                    Match = new Match { Keyword = fileItemId.ToString() }
                                }
                            }
                        }
                    }
                );

                _logger.LogInformation("✅ Все векторы файла {FileId} удалены", fileItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при удалении векторов файла {FileId}", fileItemId);
                throw;
            }
        }
    }
}