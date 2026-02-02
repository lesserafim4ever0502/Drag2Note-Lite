using System;
using System.Collections.Generic;

namespace Drag2Note.Models
{
    public enum NoteType
    {
        Note,
        Todo
    }

    public enum TodoStatus
    {
        Pending,
        Completed
    }

    public class MetadataItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public NoteType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string CustomTitle { get; set; } = string.Empty;
        public string PreviewText { get; set; } = string.Empty;
        public TodoStatus Status { get; set; } = TodoStatus.Pending;
        public int OrderIndex { get; set; }
        public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class AppData
    {
        public List<MetadataItem> Items { get; set; } = new List<MetadataItem>();
    }
}
