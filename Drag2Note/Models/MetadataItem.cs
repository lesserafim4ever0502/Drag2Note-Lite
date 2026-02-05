using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        // Helper for UI
        public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeMilliseconds(CreatedAt).LocalDateTime;
        public string CreatedAtString => CreatedAtDateTime.ToString("yyyy-MM-dd HH:mm");

        public ObservableCollection<string> Tags { get; set; } = new ObservableCollection<string>();
    }

    public class AppData
    {
        public List<MetadataItem> Items { get; set; } = new List<MetadataItem>();
    }
}
