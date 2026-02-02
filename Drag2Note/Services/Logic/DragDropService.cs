using System;
using System.Windows;
using Drag2Note.Models;

namespace Drag2Note.Services.Logic
{
    public class DragDropService
    {
        private static DragDropService _instance;
        public static DragDropService Instance => _instance ??= new DragDropService();

        private DragDropService() { }

        public DroppedContent ProcessDrop(IDataObject data)
        {
            var result = new DroppedContent();

            if (data == null) return result;

            // 1. Check for Files
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                if (data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                {
                    result.ContentType = DropContentType.Files;
                    result.FilePaths.AddRange(files);
                    return result;
                }
            }

            // 2. Check for Text / URL
            // Fix: Use UnicodeText to handle Chinese characters correctly
            if (data.GetDataPresent(DataFormats.UnicodeText))
            {
                string text = data.GetData(DataFormats.UnicodeText) as string;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Basic URL check
                    if (Uri.TryCreate(text, UriKind.Absolute, out Uri uriResult) 
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    {
                        result.ContentType = DropContentType.Url;
                        result.Url = text;
                    }
                    else
                    {
                        result.ContentType = DropContentType.Text;
                        result.TextContent = text;
                    }
                    return result;
                }
            }

            return result;
        }
    }
}
