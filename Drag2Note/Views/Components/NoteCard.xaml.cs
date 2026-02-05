using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            var window = System.Windows.Window.GetWindow(this);
            if (window?.DataContext is MainViewModel vm && DataContext is MetadataItem item)
            {
                _ = vm.SelectCard(item);
                e.Handled = true; // Prevent bubbling to DragMove
            }
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
            if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel vm && vm.IsRenaming)
            {
                vm.SaveRenameCommand.Execute(null);
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

        private void ShowTagEditor()
        {
            InlineTagEditor.Visibility = Visibility.Visible;
            InlineTagEditor.Text = "";
            InlineTagEditor.Focus();
        }

        private void InlineTagEditor_LostFocus(object sender, RoutedEventArgs e)
        {
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
                InlineTagEditor.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void SubmitTag()
        {
            if (InlineTagEditor.Visibility != Visibility.Visible) return;
            
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
