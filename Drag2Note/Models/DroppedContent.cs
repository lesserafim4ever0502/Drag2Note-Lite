using System.Collections.Generic;

namespace Drag2Note.Models
{
    public enum DropContentType
    {
        Text,
        Files,
        Url,
        Unknown
    }

    public class DroppedContent
    {
        public DropContentType ContentType { get; set; } = DropContentType.Unknown;
        public string TextContent { get; set; } = string.Empty;
        public List<string> FilePaths { get; set; } = new List<string>();
        public string Url { get; set; } = string.Empty;

        public bool IsValid => ContentType != DropContentType.Unknown;
    }
}
