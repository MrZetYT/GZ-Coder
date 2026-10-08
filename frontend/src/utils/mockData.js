// Mock данные для разработки (когда бэкенд не запущен)

export const mockFiles = [
  { 
    id: '1', 
    fileName: 'Program.cs', 
    status: 'Ready', 
    fileType: 'CSharp', 
    uploadedAt: new Date().toISOString(),
    errorMessage: null
  },
  { 
    id: '2', 
    fileName: 'FileLoaderService.cs', 
    status: 'Ready', 
    fileType: 'CSharp', 
    uploadedAt: new Date().toISOString(),
    errorMessage: null
  },
  { 
    id: '3', 
    fileName: 'VectorizationService.cs', 
    status: 'Vectorizing', 
    fileType: 'CSharp', 
    uploadedAt: new Date().toISOString(),
    errorMessage: null
  },
];

export const mockStats = {
  totalVectors: 1250,
  totalBlocks: 1250,
  vectorizedBlocks: 890,
  vectorizationProgress: 71.2
};

export const mockSearchResults = [
  {
    infoBlockId: '1',
    similarity: 0.95,
    content: 'public void SaveFiles(List<IFormFile> files) { ... }',
    blockType: 'Method',
    className: 'FileLoaderService',
    methodName: 'SaveFiles',
    startLine: 45,
    endLine: 78,
    fileName: 'FileLoaderService.cs',
    fileType: 'CSharp'
  },
  {
    infoBlockId: '2',
    similarity: 0.87,
    content: 'private async Task ProcessFileAsync(Guid fileId) { ... }',
    blockType: 'Method',
    className: 'FileLoaderService',
    methodName: 'ProcessFileAsync',
    startLine: 80,
    endLine: 120,
    fileName: 'FileLoaderService.cs',
    fileType: 'CSharp'
  }
];

export const mockChatAnswers = {
  savefiles: "етод SaveFiles сохраняет загруженные файлы на диск в папку Data, создает запись в базе данных и запускает фоновую обработку через Hangfire. аждый файл проходит парсинг и векторизацию.",
  vectorization: "екторизация использует модель bge-m3 для создания эмбеддингов размером 1024. локи кода преобразуются в векторы и сохраняются в Qdrant для семантического поиска.",
  search: "оиск работает через векторное сравнение: ваш вопрос векторизуется и ищет похожие блоки кода в Qdrant. озвращаются топ-5 результатов с процентами схожести.",
  database: "спользуется PostgreSQL с Entity Framework Core. Таблицы: FileItems (файлы) и InfoBlocks (блоки кода). Строка подключения в appsettings.json.",
  default: "Я могу ответить на вопросы о проекте GZ-Coder: загрузка файлов, парсинг кода, векторизация, поиск, база данных и архитектура. адайте конкретный вопрос!"
};
