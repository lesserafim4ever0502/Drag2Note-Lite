using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Drag2Note.ViewModels;
using Drag2Note.Models;
using Drag2Note.Services.Data;

namespace Drag2Note.Views.Components
{
    public partial class NoteCard : System.Windows.Controls.UserControl
    {
        public NoteCard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Manually commits any active edits (Tag or Rename).
        /// Can be called globally (e.g. by Window) to force save.
        /// </summary>
        public void CommitEdits()
        {
            // 1. Submit Tag if open
            if (InlineTagEditor.Visibility == Visibility.Visible)
            {
                SubmitTag();
            }

            // 2. Submit Rename if active (Double check VM state)
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is MainViewModel vm && vm.IsRenaming)
            {
                vm.SaveRenameCommand.Execute(null);
            }
        }

        private void Title_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is MainViewModel vm && DataContext is MetadataItem item)
            {
                vm.SelectedItem = item;
                vm.StartRenameCommand.Execute(null);
                e.Handled = true; // Prevent event bubbling
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Defensive check: If clicking a TextBox, let it handle the event (focus, caret, etc.)
            if (e.OriginalSource is System.Windows.Controls.TextBox) return;

            // 1. Manually submit active editors (Active Detection)
            CommitEdits();

            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is MainViewModel vm && DataContext is MetadataItem item)
            {
                // 2. Select Card
                _ = vm.SelectCard(item);
            }

            // 3. Force Focus & Mark Handled
            // Explicitly focus container to ensure visual state updates
            CardBorder.Focus();
            e.Handled = true; // Prevent bubbling to DragMove
        }

        private void Grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is MainViewModel vm && DataContext is MetadataItem item)
            {
                vm.SelectedItem = item;
                vm.OpenDetailCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void TitleEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel vm)
            {
                // Unconditionally try to save if visible, let VM handle logic
                vm.SaveRenameCommand.Execute(null);
            }
        }

        private void TitleEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel vm)
                {
                    vm.SaveRenameCommand.Execute(null);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel vm)
                {
                    vm.IsRenaming = false; // Cancel
                }
                e.Handled = true;
            }
        }

        private void TitleEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && sender is System.Windows.Controls.TextBox textBox)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        private void TagBarContainer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowTagEditor();
                e.Handled = true;
            }
        }

        private void TagItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is string tag)
            {
                // Start drag drop for tag reordering
                System.Windows.DataObject data = new System.Windows.DataObject("TagString", tag);
                System.Windows.DragDrop.DoDragDrop(element, data, System.Windows.DragDropEffects.Move);
                e.Handled = true;
            }
        }

        private void TagBarContainer_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("TagString") && DataContext is MetadataItem item)
            {
                string droppedTag = e.Data.GetData("TagString") as string;
                if (droppedTag == null) return;

                int oldIndex = item.Tags.IndexOf(droppedTag);
                if (oldIndex != -1)
                {
                    item.Tags.RemoveAt(oldIndex);
                    
                    // Simple logic: add to the end of the current tags
                    item.Tags.Add(droppedTag);
                    
                    // Notify refresh
                    TagsItemsControl.ItemsSource = null;
                    TagsItemsControl.ItemsSource = item.Tags;

                    // Trigger Save via ViewModel
                    var window = System.Windows.Window.GetWindow(this);
                    if (window?.DataContext is MainViewModel vm)
                    {
                        _ = MetadataService.Instance.SaveDataAsync();
                    }
                }
                e.Handled = true;
            }
        }

        private void BtnShowAddTag_Click(object sender, RoutedEventArgs e)
        {
            ShowTagEditor();
        }

        private bool _isSubmittingTag = false;

        private void ShowTagEditor()
        {
            _isSubmittingTag = false;
            InlineTagEditor.Visibility = Visibility.Visible;
            InlineTagEditor.Text = "";
            InlineTagEditor.Focus();
        }

        private void InlineTagEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            // When focus is lost, we should try to submit
            SubmitTag();
        }

        private void InlineTagEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitTag();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _isSubmittingTag = true; // Prevent submission on lost focus
                InlineTagEditor.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void SubmitTag()
        {
            if (_isSubmittingTag) return;
            _isSubmittingTag = true;

            string newTag = InlineTagEditor.Text.Trim();
            InlineTagEditor.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrWhiteSpace(newTag))
            {
                var window = System.Windows.Window.GetWindow(this);
                if (window?.DataContext is MainViewModel vm && DataContext is MetadataItem item)
                {
                    vm.SelectedItem = item;
                    vm.AddTagCommand.Execute(newTag);
                }
            }
        }
    }
}
