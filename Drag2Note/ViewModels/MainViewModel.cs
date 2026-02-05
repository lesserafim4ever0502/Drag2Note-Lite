using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using Drag2Note.Models;
using Drag2Note.Services.Data;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Drag2Note.Services.Logic;
using Drag2Note.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace Drag2Note.ViewModels
{
    public enum MainTab
    {
        Notes,
        Todo
    }

    public partial class MainViewModel : ObservableObject
    {
        private MainTab _currentTab = MainTab.Todo;
        public MainTab CurrentTab
        {
            get => _currentTab;
            set
            {
                if (SetProperty(ref _currentTab, value))
                {
                    RefreshFilteredItems();
                }
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    RefreshFilteredItems();
                }
            }
        }

        [ObservableProperty]
        private bool _isEditorActive;

        [ObservableProperty]
        private bool _isPreviewMode;

        [ObservableProperty]
        private bool _isRenaming;

        [ObservableProperty]
        private string _renamingText = "";

        [ObservableProperty]
        private string editorContent = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MarkdownBlock> previewBlocks = new();

        [ObservableProperty]
        private MetadataItem? _selectedItem;

        [ObservableProperty]
        private bool _isSelectionMode;

        partial void OnSelectedItemChanged(MetadataItem? value)
        {
            if (value != null && IsSelectionMode && _stashedContents != null)
            {
                _ = ExecuteInsertionAsync(value);
            }
        }

        private List<DroppedContent>? _stashedContents;

        [ObservableProperty]
        private string _selectionStatusText = "Please select a card to insert content...";

        public ObservableCollection<MetadataItem> FilteredItems { get; } = new();
        
        private readonly Dictionary<string, string> _resolvedPathCache = new();

        public ICommand SwitchToNotesCommand { get; }
        public ICommand SwitchToTodoCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand OpenDetailCommand { get; }
        public IRelayCommand<List<DroppedContent>> SetSelectionModeCommand { get; }
        public IRelayCommand CancelSelectionCommand { get; }
        public IAsyncRelayCommand<MetadataItem> CardClickCommand { get; }
        public IAsyncRelayCommand<MetadataItem> ToggleTodoStatusCommand { get; }
        public IRelayCommand SaveEditorCommand { get; }
        public IRelayCommand DeleteItemCommand { get; }
        public IRelayCommand StartRenameCommand { get; }
        public IRelayCommand SaveRenameCommand { get; }
        public IRelayCommand CloseEditorCommand { get; }
        public IRelayCommand FocusSearchCommand { get; }
        public IRelayCommand CloseWindowCommand { get; }
        public IRelayCommand MinimizeWindowCommand { get; }
        public IRelayCommand ToggleMaximizeCommand { get; }
        public IRelayCommand OpenInExplorerCommand { get; }
        public IRelayCommand OpenInSystemEditorCommand { get; }
        public IRelayCommand OpenLinkCommand { get; }
        public IRelayCommand SetEditModeCommand { get; }
        public IRelayCommand SetPreviewModeCommand { get; }
        public IRelayCommand<string> AddTagCommand { get; }
        public IRelayCommand<string> RemoveTagCommand { get; }

        public MainViewModel()
        {
            SwitchToNotesCommand = new RelayCommand(() => CurrentTab = MainTab.Notes);
            SwitchToTodoCommand = new RelayCommand(() => CurrentTab = MainTab.Todo);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            RefreshCommand = new RelayCommand(LoadData);
            OpenDetailCommand = new RelayCommand(OpenDetail);
            SetSelectionModeCommand = new RelayCommand<List<DroppedContent>>(SetSelectionMode);
            CancelSelectionCommand = new RelayCommand(CancelSelection);
            ToggleTodoStatusCommand = new AsyncRelayCommand<MetadataItem>(ToggleTodoStatusAsync);
            SaveEditorCommand = new RelayCommand(SaveEditor);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            StartRenameCommand = new RelayCommand(StartRename);
            SaveRenameCommand = new RelayCommand(SaveRename);
            CloseEditorCommand = new RelayCommand(() => 
            { 
                IsEditorActive = false;
                IsPreviewMode = false; // Reset to edit mode by default for next time
            });
            SetEditModeCommand = new RelayCommand(() => IsPreviewMode = false);
            SetPreviewModeCommand = new RelayCommand(async () => 
            { 
                IsPreviewMode = true; 
                await RefreshPreviewAsync();
            });

            AddTagCommand = new RelayCommand<string>(tag =>
            {
                if (SelectedItem == null || string.IsNullOrWhiteSpace(tag)) return;
                tag = tag.Trim();
                if (!SelectedItem.Tags.Contains(tag))
                {
                    SelectedItem.Tags.Add(tag);
                    _ = MetadataService.Instance.SaveDataAsync();
                    RefreshFilteredItems();
                }
            });

            RemoveTagCommand = new RelayCommand<string>(tag =>
            {
                if (SelectedItem == null || tag == null) return;
                if (SelectedItem.Tags.Remove(tag))
                {
                    _ = MetadataService.Instance.SaveDataAsync();
                    RefreshFilteredItems();
                }
            });
            FocusSearchCommand = new RelayCommand(() => { /* Handled in Code-behind or via interaction */ });
            
            OpenLinkCommand = new RelayCommand<MarkdownBlock>(OpenLink);
            
            OpenInSystemEditorCommand = new RelayCommand(() => 
            {
                if (SelectedItem == null) return;
                try
                {
                    Process.Start(new ProcessStartInfo(SelectedItem.FilePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open file: {ex.Message}");
                }
            });
            
            CloseWindowCommand = new RelayCommand(() => 
            {
                // Hide instead of close for better lifecycle management with tray
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is Drag2Note.Views.MainWindow)
                    {
                        window.Hide();
                        break;
                    }
                }
            });

            MinimizeWindowCommand = new RelayCommand(() => 
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is Drag2Note.Views.MainWindow)
                    {
                        window.WindowState = System.Windows.WindowState.Minimized;
                        break;
                    }
                }
            });

            ToggleMaximizeCommand = new RelayCommand(() =>
            {
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is Drag2Note.Views.MainWindow)
                    {
                        window.WindowState = window.WindowState == System.Windows.WindowState.Maximized 
                            ? System.Windows.WindowState.Normal 
                            : System.Windows.WindowState.Maximized;
                        break;
                    }
                }
            });

            OpenInExplorerCommand = new RelayCommand(() =>
            {
                if (SelectedItem != null && !string.IsNullOrEmpty(SelectedItem.FilePath))
                {
                    try
                    {
                        string folder = System.IO.Path.GetDirectoryName(SelectedItem.FilePath);
                        if (System.IO.Directory.Exists(folder))
                        {
                            System.Diagnostics.Process.Start("explorer.exe", folder);
                        }
                    }
                    catch { }
                }
            });

            MetadataService.Instance.DataChanged += (s, e) => 
            {
                App.Current.Dispatcher.Invoke(() => RefreshFilteredItems());
            };

            SettingsService.Instance.SettingsChanged += (s, e) => 
            {
                App.Current.Dispatcher.Invoke(() => RefreshFilteredItems());
            };

            LoadData();
        }

        public void LoadData()
        {
            MetadataService.Instance.Reload();
            RefreshFilteredItems();
        }

        public async Task SelectCard(MetadataItem item)
        {
            if (IsSelectionMode)
            {
                // In selection mode, clicking a card (even if already active) should trigger insertion
                SelectedItem = item;
                if (_stashedContents != null)
                {
                    await ExecuteInsertionAsync(item);
                }
            }
            else
            {
                if (IsRenaming) return;
                SelectedItem = item;
            }
        }

        private void RefreshFilteredItems()
        {
            var allItems = MetadataService.Instance.GetData().Items;
            
            var filtered = allItems.Where(item => 
            {
                // Tab Filter
                bool matchTab = (CurrentTab == MainTab.Notes && item.Type == NoteType.Note) ||
                                (CurrentTab == MainTab.Todo && item.Type == NoteType.Todo);
                
                if (!matchTab) return false;

                // Search Filter
                if (string.IsNullOrWhiteSpace(SearchText)) return true;

                string search = SearchText.Trim().ToLower();
                
                string displayTitle = string.IsNullOrEmpty(item.CustomTitle) ? "Untitled" : item.CustomTitle;
                bool matchTitle = displayTitle.ToLower().Contains(search);
                bool matchPreview = item.PreviewText?.ToLower().Contains(search) ?? false;
                bool matchTags = item.Tags != null && item.Tags.Any(t => t.ToLower().Contains(search));

                return matchTitle || matchPreview || matchTags;
            })
            .OrderBy(item => SettingsService.Instance.GetSettings().MoveCompletedToBottom && item.Status == TodoStatus.Completed)
            .ThenByDescending(item => item.OrderIndex)
            .ThenByDescending(item => item.CreatedAt)
            .ToList();

            FilteredItems.Clear();
            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }
        }

        public void SetSelectionMode(List<DroppedContent>? contents)
        {
            _stashedContents = contents;
            IsSelectionMode = true;
            IsEditorActive = false; // Close editor if open
        }

        private void CancelSelection()
        {
            IsSelectionMode = false;
            _stashedContents = null;
        }

        // Removed HandleCardClickAsync in favor of OnSelectedItemChanged

        private async Task ExecuteInsertionAsync(MetadataItem target)
        {
            try 
            {
                string targetFolder = Path.GetDirectoryName(target.FilePath)!;
                string bundle = await StorageService.Instance.BundleResourcesAsync(targetFolder, _stashedContents!);
                
                await StorageService.Instance.AppendToMarkdownAsync(target.FilePath, bundle);
                
                // Update preview text from the actual file content now
                string updatedContent = await File.ReadAllTextAsync(target.FilePath, Encoding.UTF8);
                target.PreviewText = StorageService.Instance.GeneratePreviewText(updatedContent);
                await MetadataService.Instance.SaveDataAsync();
                
                CancelSelection();
                // Refresh detail if target was the one being edited (though we closed editor)
                // Just reload data to be safe
                MetadataService.Instance.Reload();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Insertion failed: {ex.Message}");
            }
        }

        private async void OpenDetail()
        {
            if (SelectedItem == null) return;
            EditorContent = await StorageService.Instance.LoadMarkdownAsync(SelectedItem.Type, SelectedItem.Id);
            IsEditorActive = true;
            IsPreviewMode = true; // Default to preview
            _ = RefreshPreviewAsync();
        }

        private void OpenLink(MarkdownBlock? block)
        {
            if (block == null || string.IsNullOrEmpty(block.ResolvedPath)) return;
            try
            {
                string path = block.ResolvedPath;
                if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    path = new Uri(path).LocalPath;
                }
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open link: {ex.Message}");
            }
        }

        private async Task RefreshPreviewAsync()
        {
            if (SelectedItem == null || string.IsNullOrEmpty(EditorContent)) return;

            PreviewBlocks.Clear();

            var lines = EditorContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string itemFolder = Path.Combine(StorageService.Instance.GetTypePath(SelectedItem.Type), SelectedItem.Id);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Check for Separator ---
                if (line.Trim() == "---")
                {
                    PreviewBlocks.Add(new MarkdownBlock { Type = MarkdownBlockType.Separator });
                    continue;
                }

                // Check for Todo Item - [ ] or - [x]
                var todoMatch = Regex.Match(line, @"^-\s\[([x\s])\]\s*(.*)", RegexOptions.IgnoreCase);
                if (todoMatch.Success)
                {
                    PreviewBlocks.Add(new MarkdownBlock 
                    { 
                        Type = MarkdownBlockType.TodoItem, 
                        Content = todoMatch.Groups[2].Value,
                        Target = todoMatch.Groups[1].Value.ToLower() == "x" ? "Checked" : "Unchecked"
                    });
                    continue;
                }

                // Check for Image ![(alt)](target)
                var imgMatch = Regex.Match(line, @"!\[(.*?)\]\((.*?)\)", RegexOptions.IgnoreCase);
                if (imgMatch.Success)
                {
                    // ... (existing image logic)
                    string target = imgMatch.Groups[2].Value.Trim();
                    string cacheKey = $"{SelectedItem.Id}_{target}";
                    
                    if (!_resolvedPathCache.TryGetValue(cacheKey, out string resolved))
                    {
                        string localPath = Path.Combine(itemFolder, target);
                        if (target.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                        {
                            resolved = await StorageService.Instance.ResolveShortcutAsync(localPath);
                        }
                        else if (File.Exists(localPath))
                        {
                            resolved = localPath;
                        }
                        else
                        {
                            resolved = target;
                        }
                        _resolvedPathCache[cacheKey] = resolved;
                    }
                    
                    string finalPath = resolved;
                    if (!string.IsNullOrEmpty(finalPath) && !finalPath.StartsWith("http") && !finalPath.StartsWith("file:"))
                    {
                        try { finalPath = new Uri(finalPath).AbsoluteUri; } catch { }
                    }

                    PreviewBlocks.Add(new MarkdownBlock 
                    { 
                        Type = MarkdownBlockType.Image, 
                        Content = imgMatch.Groups[1].Value, 
                        Target = target, 
                        ResolvedPath = finalPath 
                    });
                    continue;
                }

                // Check for Link [(text)](target)
                var linkMatch = Regex.Match(line, @"\[(.*?)\]\((.*?)\)");
                if (linkMatch.Success)
                {
                    string target = linkMatch.Groups[2].Value;
                    string resolved = target;
                    if (target.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        resolved = await StorageService.Instance.ResolveShortcutAsync(Path.Combine(itemFolder, target.Trim()));
                    }

                    PreviewBlocks.Add(new MarkdownBlock 
                    { 
                        Type = MarkdownBlockType.Link, 
                        Content = linkMatch.Groups[1].Value, 
                        Target = target, 
                        ResolvedPath = resolved 
                    });
                    continue;
                }

                // Check for Plain URL (http/https/www or domain-like strings)
                var urlMatch = Regex.Match(line, @"((https?://|www\.)[^\s]+|[a-zA-Z0-9-]+\.(?:com|net|org|cn|io|me|info|gov|edu)(?:/[^\s]*)?)", RegexOptions.IgnoreCase);
                if (urlMatch.Success)
                {
                    string url = urlMatch.Value;
                    string finalUrl = url;
                    if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        finalUrl = "https://" + url;
                    }

                    PreviewBlocks.Add(new MarkdownBlock 
                    { 
                        Type = MarkdownBlockType.Link, 
                        Content = url, 
                        Target = finalUrl, 
                        ResolvedPath = finalUrl 
                    });
                    continue;
                }

                // Plain Text
                PreviewBlocks.Add(new MarkdownBlock 
                { 
                    Type = MarkdownBlockType.Text, 
                    Content = line 
                });
            }
        }

        private async void SaveEditor()
        {
            if (SelectedItem == null || !IsEditorActive) return;
            
            await StorageService.Instance.UpdateMarkdownAsync(SelectedItem.Type, SelectedItem.Id, EditorContent);
            
            // Update preview text in metadata from the full content
            SelectedItem.PreviewText = StorageService.Instance.GeneratePreviewText(EditorContent);
            await MetadataService.Instance.SaveDataAsync();
        }

        private void StartRename()
        {
            if (SelectedItem == null) return;
            RenamingText = string.IsNullOrEmpty(SelectedItem.CustomTitle) ? "Untitled" : SelectedItem.CustomTitle;
            IsRenaming = true;
        }

        private async void SaveRename()
        {
            if (SelectedItem == null || !IsRenaming) return;
            
            SelectedItem.CustomTitle = RenamingText;
            // TODO: Actually rename the directory if needed, or just update metadata
            await MetadataService.Instance.SaveDataAsync();
            
            IsRenaming = false;
            RefreshFilteredItems();
        }

        private async void DeleteItem()
        {
            if (SelectedItem == null) return;
            
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete this item?", 
                "Confirm Delete", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                var itemToDelete = SelectedItem;
                StorageService.Instance.DeleteItemDirectory(itemToDelete.Type, itemToDelete.Id);
                await MetadataService.Instance.RemoveItemAsync(itemToDelete.Id);
                LoadData();
            }
        }

        private void OpenSettings()
        {
            Services.WindowManager.Instance.OpenSettings();
        }

        public void CycleTab(bool right)
        {
            if (IsEditorActive) return; 
            
            if (right)
            {
                if (CurrentTab == MainTab.Notes) CurrentTab = MainTab.Todo;
            }
            else
            {
                if (CurrentTab == MainTab.Todo) CurrentTab = MainTab.Notes;
            }
        }

        public async void ReorderItem(MetadataItem dragged, int targetIndex)
        {
            if (dragged == null || targetIndex < 0 || targetIndex >= FilteredItems.Count) return;

            int oldIndex = FilteredItems.IndexOf(dragged);
            if (oldIndex == -1 || oldIndex == targetIndex) return;

            // Move in the filtered collection first for immediate UI feedback
            FilteredItems.Move(oldIndex, targetIndex);

            // Re-calculate OrderIndex for ALL items in the current filtered list
            // We want the first item to have the highest OrderIndex
            int count = FilteredItems.Count;
            for (int i = 0; i < count; i++)
            {
                FilteredItems[i].OrderIndex = count - i;
            }

            // Save to metadata
            await MetadataService.Instance.SaveDataAsync();
        }

        private async Task ToggleTodoStatusAsync(Models.MetadataItem? item)
        {
            if (item == null || item.Type != Models.NoteType.Todo) return;

            item.Status = item.Status == Models.TodoStatus.Pending ? Models.TodoStatus.Completed : Models.TodoStatus.Pending;
            
            // Sync with file
            await StorageService.Instance.SyncTodoStatusToFileAsync(item.FilePath, item.Status == Models.TodoStatus.Completed);
            
            // Save metadata
            await MetadataService.Instance.SaveDataAsync();
            
            // Sort and refresh
            RefreshFilteredItems();
        }
    }
}
