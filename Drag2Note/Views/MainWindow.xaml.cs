using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Drag2Note.ViewModels;
using Drag2Note.Models;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System;
using Drag2Note.Services;

namespace Drag2Note.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _autoHideTimer;
        private bool _isDocked = false;
        private double _originalLeft;
        private double _dockedVisibleWidth = 3.0; // Show a tiny sliver when hidden
        private bool _isAnimating = false;

        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += MainWindow_SizeChanged;
            
            this.Loaded += (s, e) => {
                ItemsListBox.Focus();
                UpdateDockState();
            };
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
            
            // Lock ListBox navigation
            ItemsListBox.PreviewKeyDown += ItemsListBox_PreviewKeyDown;

            // Auto-hide detection
            this.LocationChanged += MainWindow_LocationChanged;
            
            _autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _autoHideTimer.Tick += AutoHideTimer_Tick;
            _autoHideTimer.Start();

            // Listen for settings changes
            SettingsService.Instance.SettingsChanged += OnSettingsChanged;
            this.Closed += (s, e) => 
            {
                _autoHideTimer.Stop();
                SettingsService.Instance.SettingsChanged -= OnSettingsChanged;
            };
        }

        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => 
            {
                var settings = SettingsService.Instance.GetSettings();
                if (!settings.AutoHideAtEdge && _isDocked)
                {
                    _isDocked = false;
                    ShowWithAnimation();
                }
                else if (settings.AutoHideAtEdge)
                {
                    UpdateDockState();
                }
            });
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            UpdateDockState();
        }

        private void UpdateDockState()
        {
            if (SettingsService.Instance.GetSettings().AutoHideAtEdge)
            {
                var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                bool wasDocked = _isDocked;
                
                // Dock only if near horizontal edges
                if (Left <= screen.WorkingArea.Left + 20)
                {
                    _isDocked = true;
                    _originalLeft = screen.WorkingArea.Left;
                }
                else if (Left + Width >= screen.WorkingArea.Right - 20)
                {
                    _isDocked = true;
                    _originalLeft = screen.WorkingArea.Right - Width;
                }
                else
                {
                    _isDocked = false;
                }
            }
            else
            {
                _isDocked = false;
            }
        }

        private void AutoHideTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isDocked || _isAnimating) return;

            bool isMouseOver = IsMouseOverWindow();

            if (isMouseOver && Math.Abs(Left - _originalLeft) > 5)
            {
                ShowWithAnimation();
            }
            else if (!isMouseOver && Math.Abs(Left - _originalLeft) < 5)
            {
                HideWithAnimation();
            }
        }

        private bool IsMouseOverWindow()
        {
            var mousePos = GetMousePosition();
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            
            // If hidden on left
            if (Left < screen.WorkingArea.Left + 20)
            {
                return mousePos.X >= 0 && mousePos.X <= Left + Width + 20 && 
                       mousePos.Y >= Top && mousePos.Y <= Top + Height;
            }
            // If hidden on right
            if (Left + Width > screen.WorkingArea.Right - 20)
            {
                return mousePos.X >= Left - 20 && mousePos.X <= screen.WorkingArea.Right && 
                       mousePos.Y >= Top && mousePos.Y <= Top + Height;
            }

            return this.IsMouseOver;
        }

        private System.Windows.Point GetMousePosition()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }

        private void ShowWithAnimation()
        {
            _isAnimating = true;
            DoubleAnimation anim = new DoubleAnimation(_originalLeft, TimeSpan.FromMilliseconds(200));
            anim.Completed += (s, e) => 
            {
                _isAnimating = false;
                this.BeginAnimation(Window.LeftProperty, null);
                this.Left = _originalLeft;
            };
            this.BeginAnimation(Window.LeftProperty, anim);
        }

        private void HideWithAnimation()
        {
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            double targetLeft;

            if (_originalLeft <= screen.WorkingArea.Left + 20)
            {
                targetLeft = _originalLeft - Width + _dockedVisibleWidth;
            }
            else
            {
                targetLeft = screen.WorkingArea.Right - _dockedVisibleWidth;
            }

            _isAnimating = true;
            DoubleAnimation anim = new DoubleAnimation(targetLeft, TimeSpan.FromMilliseconds(300));
            anim.Completed += (s, e) => 
            {
                _isAnimating = false;
                this.BeginAnimation(Window.LeftProperty, null);
                this.Left = targetLeft;
            };
            this.BeginAnimation(Window.LeftProperty, anim);
        }

        private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 0. Ignore if clicking ON a TextBox (let it handle input/focus)
            if (e.OriginalSource is System.Windows.Controls.TextBox) return;

            // 1. Try to find the currently focused NoteCard and Force Commit
            var focused = System.Windows.Input.Keyboard.FocusedElement as DependencyObject;
            if (focused is System.Windows.Controls.TextBox)
            {
                var card = FindParent<Drag2Note.Views.Components.NoteCard>(focused);
                if (card != null)
                {
                    card.CommitEdits();
                }
                else if (DataContext is MainViewModel vm && vm.IsRenaming)
                {
                    // Fallback: If focused is a TextBox but not in a card (e.g. somehow?), ensure Rename saves
                    vm.SaveRenameCommand.Execute(null);
                }
            }
            
            // 2. Force Global Focus Clear (Since we clicked background)
            // This ensures visual states update (e.g. selection borders might change)
            System.Windows.Input.Keyboard.ClearFocus();
            this.Focus();
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T t) return t;
                child = System.Windows.Media.VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void MainWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // Lock width to 400, only allow height changes
            if (e.NewSize.Width != 400)
            {
                this.Width = 400;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only DragMove if we are clicking on background or top header area (Row 0)
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                // Prevent DragMove if we are interacting with ListBox or other inputs
                var pos = e.GetPosition(this);
                if (pos.Y < 60) // Simple header height check
                {
                    this.DragMove();
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            // ESC key logic:
            if (e.Key == Key.Escape)
            {
                if (vm != null && vm.IsEditorActive)
                {
                    vm.CloseEditorCommand.Execute(null);
                    // Ensure focus is restored to the list box to enable arrow keys
                    Dispatcher.BeginInvoke(new System.Action(() => 
                    {
                        if (ItemsListBox.SelectedItem != null)
                        {
                            var container = ItemsListBox.ItemContainerGenerator.ContainerFromItem(ItemsListBox.SelectedItem) as System.Windows.Controls.ListBoxItem;
                            if (container != null)
                            {
                                container.Focus();
                                System.Windows.Input.Keyboard.Focus(container);
                            }
                            else
                            {
                                ItemsListBox.Focus();
                            }
                        }
                        else
                        {
                            ItemsListBox.Focus();
                        }
                    }), System.Windows.Threading.DispatcherPriority.Input);
                }
                else
                {
                    this.Hide();
                }
                e.Handled = true;
                return;
            }

            // Ctrl+Alt+Q focus search - Use Preview to catch it everywhere
            if (e.Key == Key.Q && 
                (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (vm != null)
            {
                if (e.Key == Key.Left)
                {
                    if (vm.IsEditorActive)
                    {
                        // Switch to Edit mode if not typing
                        if (!(System.Windows.Input.FocusManager.GetFocusedElement(this) is System.Windows.Controls.TextBox)) vm.SetEditModeCommand.Execute(null);
                    }
                    else
                    {
                        vm.CycleTab(false);
                    }
                    e.Handled = true;
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    if (vm.IsEditorActive)
                    {
                        // Switch to Preview mode if not typing
                        if (!(System.Windows.Input.FocusManager.GetFocusedElement(this) is System.Windows.Controls.TextBox)) vm.SetPreviewModeCommand.Execute(null);
                    }
                    else
                    {
                        vm.CycleTab(true);
                    }
                    e.Handled = true;
                    return;
                }
            }

            // Default selection logic if nothing is selected
            if (e.Key == Key.Down && ItemsListBox.SelectedItem == null && ItemsListBox.Items.Count > 0)
            {
                ItemsListBox.SelectedIndex = 0;
                // Move focus to the selected container
                var container = ItemsListBox.ItemContainerGenerator.ContainerFromIndex(0) as System.Windows.Controls.ListBoxItem;
                container?.Focus();
                e.Handled = true;
                return;
            }
        }

        private void ItemsListBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                // If we are on the first item and press Up, don't jump out
                if (e.Key == Key.Up && ItemsListBox.SelectedIndex == 0)
                {
                    e.Handled = true;
                    return;
                }
                // If we are on the last item and press Down, don't jump out
                if (e.Key == Key.Down && ItemsListBox.SelectedIndex == ItemsListBox.Items.Count - 1)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private System.Windows.Point _dragStartPoint;
        private void ItemsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ItemsListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentPos = e.GetPosition(null);
                if (System.Math.Abs(currentPos.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    System.Math.Abs(currentPos.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    System.Windows.Controls.ListBox listBox = sender as System.Windows.Controls.ListBox;
                    System.Windows.Controls.ListBoxItem listBoxItem = FindVisualParent<System.Windows.Controls.ListBoxItem>((DependencyObject)e.OriginalSource);

                    if (listBoxItem != null)
                    {
                        MetadataItem data = (MetadataItem)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                        System.Windows.DataObject dragData = new System.Windows.DataObject("MetadataItem", data);
                        System.Windows.DragDrop.DoDragDrop(listBoxItem, dragData, System.Windows.DragDropEffects.Move);
                    }
                }
            }
        }

        private void ItemsListBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("MetadataItem"))
            {
                MetadataItem droppedData = e.Data.GetData("MetadataItem") as MetadataItem;
                System.Windows.Controls.ListBox listBox = sender as System.Windows.Controls.ListBox;
                System.Windows.Controls.ListBoxItem targetItem = FindVisualParent<System.Windows.Controls.ListBoxItem>((DependencyObject)e.OriginalSource);

                if (droppedData != null && DataContext is MainViewModel vm)
                {
                    int targetIndex = -1;
                    if (targetItem != null)
                    {
                        targetIndex = listBox.ItemContainerGenerator.IndexFromContainer(targetItem);
                    }
                    else
                    {
                        // dropped at the end of the list
                        targetIndex = listBox.Items.Count;
                    }

                    if (targetIndex != -1)
                    {
                        vm.ReorderItem(droppedData, targetIndex);
                    }
                }
            }
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            return FindVisualParent<T>(parentObject);
        }
    }
}
