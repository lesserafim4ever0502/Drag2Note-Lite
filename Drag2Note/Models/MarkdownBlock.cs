using System.Collections.Generic;

namespace Drag2Note.Models
{
    public enum MarkdownBlockType
    {
        Text,
        Image,
        Link,
        Separator,
        TodoItem
    }

    public class MarkdownBlock
    {
        public MarkdownBlockType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Target { get; set; } // LNK path or URL
        public string? ResolvedPath { get; set; } // Actual file path after resolving LNK
    }
}
