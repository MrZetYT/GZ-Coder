using RAG_Code_Base.Models;
using TreeSitter;

namespace RAG_Code_Base.Services.Parsers.TreeSitterParsers
{
    public abstract class BaseTreeSitterParser : IFileParser
    {
        protected abstract string GetLanguageName();
        protected abstract string[] GetFunctionNodeTypes();
        protected abstract string[] GetClassNodeTypes();
        protected abstract string[] GetInterfaceNodeTypes();
        protected abstract string[] GetEnumNodeTypes();

        protected abstract string? ExtractFunctionName(Node node);
        protected abstract string? ExtractClassName(Node node);

        public List<InfoBlock> Parse(FileItem fileItem)
        {
            string code = File.ReadAllText(fileItem.FilePath);

            using var language = new Language(GetLanguageName());
            using var parser = new Parser { Language = language };
            using var tree = parser.Parse(code);

            if (tree?.RootNode == null)
            {
                return new List<InfoBlock>();
            }

            var blocks = new List<InfoBlock>();
            CollectNodes(tree.RootNode, fileItem.Id, blocks, fileItem);

            return blocks;
        }

        private void CollectNodes(Node node, Guid fileItemId, List<InfoBlock> blocks, FileItem fileItem)
        {
            string nodeType = node.Type;
            bool found = false;

            if (GetFunctionNodeTypes().Contains(nodeType))
            {
                blocks.Add(CreateFunctionBlock(node, fileItemId));
                found = true;
            }
            else if (GetClassNodeTypes().Contains(nodeType))
            {
                blocks.Add(CreateClassBlock(node, fileItemId));
                found = true;
            }
            else if (GetInterfaceNodeTypes().Contains(nodeType))
            {
                blocks.Add(CreateInterfaceBlock(node, fileItemId));
                found = true;
            }
            else if (GetEnumNodeTypes().Contains(nodeType))
            {
                blocks.Add(CreateEnumBlock(node, fileItemId));
                found = true;
            }

            foreach (var child in node.NamedChildren)
            {
                CollectNodes(child, fileItemId, blocks, fileItem);
            }

            if (!found && (GetLanguageName() == "HTML" || GetLanguageName() == "CSS") && blocks.Count == 0)
            {
                string fullContent = File.ReadAllText(fileItem.FilePath);

                blocks.Add(new InfoBlock
                {
                    FileItemId = fileItemId,
                    Content = fullContent,
                    BlockType = "Content",
                    StartLine = 1,
                    EndLine = fullContent.Split('\n').Length,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }


        private InfoBlock CreateFunctionBlock(Node node, Guid fileItemId)
        {
            return new InfoBlock
            {
                FileItemId = fileItemId,
                Content = node.Text,
                BlockType = "Method",
                MethodName = ExtractFunctionName(node),
                ClassName = FindParentClassName(node),
                StartLine = node.StartPosition.Row + 1,
                EndLine = node.EndPosition.Row + 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        private InfoBlock CreateClassBlock(Node node, Guid fileItemId)
        {
            return new InfoBlock
            {
                FileItemId = fileItemId,
                Content = node.Text,
                BlockType = "Class",
                ClassName = ExtractClassName(node),
                StartLine = node.StartPosition.Row + 1,
                EndLine = node.EndPosition.Row + 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        private InfoBlock CreateInterfaceBlock(Node node, Guid fileItemId)
        {
            return new InfoBlock
            {
                FileItemId = fileItemId,
                Content = node.Text,
                BlockType = "Interface",
                ClassName = ExtractClassName(node),
                StartLine = node.StartPosition.Row + 1,
                EndLine = node.EndPosition.Row + 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        private InfoBlock CreateEnumBlock(Node node, Guid fileItemId)
        {
            return new InfoBlock
            {
                FileItemId = fileItemId,
                Content = node.Text,
                BlockType = "Enum",
                ClassName = ExtractClassName(node),
                StartLine = node.StartPosition.Row + 1,
                EndLine = node.EndPosition.Row + 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        private string? FindParentClassName(Node node)
        {
            var parent = node.Parent;

            while (parent != null)
            {
                if (GetClassNodeTypes().Contains(parent.Type))
                {
                    return ExtractClassName(parent);
                }

                parent = parent.Parent;
            }

            return null;
        }
    }
}