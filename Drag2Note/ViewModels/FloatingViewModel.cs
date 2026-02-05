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
using Drag2Note.Services;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalItemsCountString))]
        private int _totalItems = 0;

        public string TotalItemsCountString => $"{TotalItems} Item{(TotalItems > 1 ? "s" : "")}";

        private readonly List<DroppedContent> _accumulatedContents = new();

        public IRelayCommand NoteCommand { get; }
        public IRelayCommand TodoCommand { get; }
        public IRelayCommand InsertCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IRelayCommand ClearAllCommand { get; }

        public FloatingViewModel()
        {
            NoteCommand = new AsyncRelayCommand(() => ProcessContentAsync(NoteType.Note));
            TodoCommand = new AsyncRelayCommand(() => ProcessContentAsync(NoteType.Todo));
            InsertCommand = new AsyncRelayCommand(ProcessInsertAsync);
            CancelCommand = new RelayCommand(ResetState);
            ClearAllCommand = new RelayCommand(ResetState);
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
                _accumulatedContents.Add(content);
                TotalItems = _accumulatedContents.Count;
                CurrentState = FloatingWindowState.Decision;
                StatusText = ""; 
            }
            else
            {
                if (_accumulatedContents.Count == 0)
                {
                    StatusText = "Invalid Content";
                    Task.Delay(1000).ContinueWith(_ => ResetState(), TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    CurrentState = FloatingWindowState.Decision;
                    StatusText = "";
                }
            }
        }

        private async Task ProcessContentAsync(NoteType type)
        {
            if (_accumulatedContents.Count == 0) return;

            CurrentState = FloatingWindowState.Processing;
            StatusText = "Saving...";

            // 1. Create Metadata Item first to get ID
            var item = new MetadataItem
            {
                Type = type
            };
            
            // Create specific folder for this item
            string itemFolder = StorageService.Instance.CreateItemDirectory(type, item.Id);

            // 2. Bundle all content according to PRD
            string combinedMarkdown = await StorageService.Instance.BundleResourcesAsync(itemFolder, _accumulatedContents);

            // Set preview and title from the first meaningful item
            var firstContent = _accumulatedContents.First();
            if (firstContent.ContentType == DropContentType.Text || firstContent.ContentType == DropContentType.Url)
            {
                string text = firstContent.ContentType == DropContentType.Url ? firstContent.Url : firstContent.TextContent;
                item.PreviewText = text.Length > 50 ? text.Substring(0, 50) + "..." : text;
                string snippet = text.Trim().Split('\n')[0];
                if (snippet.Length > 30) snippet = snippet.Substring(0, 30) + "...";
                item.CustomTitle = snippet;
            }
            else if (firstContent.ContentType == DropContentType.Files)
            {
                item.PreviewText = $"{firstContent.FilePaths.Count} File(s)";
                if (firstContent.FilePaths.Count > 0)
                {
                    item.CustomTitle = Path.GetFileName(firstContent.FilePaths[0]);
                }
            }

            // Save final markdown
            string finalContent = type == NoteType.Todo ? $"- [ ] {Environment.NewLine}{combinedMarkdown}" : combinedMarkdown;
            string fullPath = await StorageService.Instance.SaveMarkdownAsync(itemFolder, finalContent);
            item.FilePath = fullPath;

            // 3. Save Metadata
            await MetadataService.Instance.AddItemAsync(item);

            // 4. Reset
            ResetState();
        }

        private async Task ProcessInsertAsync()
        {
            if (_accumulatedContents.Count == 0) return;
            
            // 1. Stash content and notify MainViewModel
            var mainVm = WindowManager.Instance.MainWindow?.DataContext as ViewModels.MainViewModel;
            if (mainVm != null)
            {
                mainVm.SetSelectionMode(new List<DroppedContent>(_accumulatedContents));
                WindowManager.Instance.ShowMainWindow();
            }

            // 2. Clear Floating Window
            ResetState();
        }

        private void ResetState()
        {
            CurrentState = FloatingWindowState.Idle;
            StatusText = "Drag Here";
            _accumulatedContents.Clear();
            TotalItems = 0;
        }
    }
}
