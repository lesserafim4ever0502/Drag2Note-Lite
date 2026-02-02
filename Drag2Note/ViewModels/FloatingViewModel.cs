using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drag2Note.Models;
using Drag2Note.Services.Logic;
using Drag2Note.Services.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Drag2Note.ViewModels
{
    public enum FloatingWindowState
    {
        Idle,
        Hover,
        Processing,
        Decision
    }

    public partial class FloatingViewModel : ObservableObject
    {
        [ObservableProperty]
        private FloatingWindowState _currentState = FloatingWindowState.Idle;

        [ObservableProperty]
        private string _statusText = "Drag Here";

        private DroppedContent _currentContent;

        public ICommand NoteCommand { get; }
        public ICommand TodoCommand { get; }
        public ICommand CancelCommand { get; }

        public FloatingViewModel()
        {
            NoteCommand = new AsyncRelayCommand(() => ProcessContentAsync(NoteType.Note));
            TodoCommand = new AsyncRelayCommand(() => ProcessContentAsync(NoteType.Todo));
            CancelCommand = new RelayCommand(ResetState);
        }

        public void OnDragEnter()
        {
            if (CurrentState == FloatingWindowState.Idle)
            {
                CurrentState = FloatingWindowState.Hover;
                StatusText = "Drop to Save";
            }
        }

        public void OnDragLeave()
        {
            if (CurrentState == FloatingWindowState.Hover)
            {
                ResetState();
            }
        }

        public void OnDrop(System.Windows.IDataObject data)
        {
            CurrentState = FloatingWindowState.Processing;
            StatusText = "Processing...";

            var content = DragDropService.Instance.ProcessDrop(data);
            if (content.IsValid)
            {
                _currentContent = content;
                CurrentState = FloatingWindowState.Decision;
                StatusText = ""; 
            }
            else
            {
                StatusText = "Invalid Content";
                Task.Delay(1000).ContinueWith(_ => ResetState(), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private async Task ProcessContentAsync(NoteType type)
        {
            if (_currentContent == null || !_currentContent.IsValid) return;

            CurrentState = FloatingWindowState.Processing;
            StatusText = "Saving...";

            // 1. Create Metadata Item
            var item = new MetadataItem
            {
                Type = type,
                Tags = new List<string>() // Empty for now
            };

            // 2. Handle Content
            if (_currentContent.ContentType == DropContentType.Text || _currentContent.ContentType == DropContentType.Url)
            {
                string content = _currentContent.ContentType == DropContentType.Url ? _currentContent.Url : _currentContent.TextContent;
                item.PreviewText = content.Length > 50 ? content.Substring(0, 50) + "..." : content;
                
                // Save text to MD file (pass type)
                // Returns full path now
                string fullPath = await StorageService.Instance.SaveMarkdownAsync(content, type);
                item.FilePath = fullPath;
            }
            else if (_currentContent.ContentType == DropContentType.Files)
            {
                item.PreviewText = $"{_currentContent.FilePaths.Count} File(s)";
                
                // Create Shortcuts
                foreach (var path in _currentContent.FilePaths)
                {
                    string name = Path.GetFileName(path) + ".lnk";
                    // Pass type to shortcut creation to put in correct folder
                    await StorageService.Instance.CreateShortcutAsync(path, name, type);
                }
                
                string fileListMd = string.Join(Environment.NewLine, _currentContent.FilePaths.Select(p => p)); 
                string fullPath = await StorageService.Instance.SaveMarkdownAsync(fileListMd, type);
                item.FilePath = fullPath;
            }

            // 3. Save Metadata
            await MetadataService.Instance.AddItemAsync(item);

            // 4. Reset
            ResetState();
        }

        private void ResetState()
        {
            CurrentState = FloatingWindowState.Idle;
            StatusText = "Drag Here";
            _currentContent = null;
        }
    }
}
