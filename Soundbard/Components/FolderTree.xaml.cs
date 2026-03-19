using Soundbard.Models;
using Soundbard.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;

namespace Soundbard.Components
{
    public partial class FolderTree : UserControl
    {
        private FolderNode? _draggedNode;

        /// <summary>
        /// Initializes the FolderTree component and sets the DataContext to a new FolderTreeViewModel.
        /// </summary>
        public FolderTree()
        {
            InitializeComponent();
            DataContext = new FolderTreeViewModel();
        }

        /// <summary>
        /// Toggles the expanded state of a TreeViewItem when its header border is clicked.
        /// </summary>
        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var tvi = border?.TemplatedParent as TreeViewItem
                      ?? FindParent<TreeViewItem>(border);
            if (tvi != null)
                tvi.IsExpanded = !tvi.IsExpanded;
        }

        /// <summary>
        /// Handles key input on the rename TextBox.
        /// Enter confirms the rename, Escape cancels it.
        /// </summary>
        private void RenameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CommitRename(sender as TextBox);
            else if (e.Key == Key.Escape)
                CancelRename(sender as TextBox);
        }

        /// <summary>
        /// Focuses and selects all text in the rename TextBox when it becomes visible.
        /// </summary>
        private void RenameBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox box && box.IsVisible)
            {
                box.Dispatcher.BeginInvoke(new Action(() =>
                {
                    box.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        box.Focus();
                        box.SelectAll();
                    }), System.Windows.Threading.DispatcherPriority.Input);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        /// <summary>
        /// Begins an inline rename on the selected folder node.
        /// </summary>
        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is FolderNode node)
            {
                node.OriginalName = node.Name;
                node.IsRenaming = true;
            }
        }

        /// <summary>
        /// Commits the rename operation, moving the folder on disk if the name has changed.
        /// Cancels silently if the name is empty or unchanged.
        /// </summary>
        private void CommitRename(TextBox? box)
        {
            if (box?.DataContext is not FolderNode node) return;
            if (!node.IsRenaming) return;
            var newName = box.Text.Trim();
            if (string.IsNullOrEmpty(newName) || newName == node.OriginalName)
            {
                node.Name = node.OriginalName;
                node.IsRenaming = false;
                return;
            }
            var extension = Path.GetExtension(node.FullPath);
            var directory = Path.GetDirectoryName(node.FullPath)!;
            var oldPath = Path.Combine(directory, node.OriginalName + (node.IsFolder ? "" : extension));
            var newPath = Path.Combine(directory, newName + (node.IsFolder ? "" : extension));
            try
            {
                if (node.IsFolder)
                    Directory.Move(oldPath, newPath);
                else
                    File.Move(oldPath, newPath);
                node.FullPath = newPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                node.Name = node.OriginalName;
            }
            node.IsRenaming = false;
        }

        /// <summary>
        /// Cancels the rename operation and restores the original folder name.
        /// </summary>
        private void CancelRename(TextBox? box)
        {
            if (box?.DataContext is FolderNode node)
            {
                node.Name = node.OriginalName;
                node.IsRenaming = false;
            }
        }

        /// <summary>
        /// Creates a new folder on disk inside the selected folder,
        /// adds it to the tree, expands the parent, and begins inline rename.
        /// </summary>
        private async void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem mi || mi.Tag is not FolderNode parent) return;
            var newPath = Path.Combine(parent.FullPath, "New Folder");
            var uniquePath = newPath;
            int counter = 1;
            while (Directory.Exists(uniquePath))
                uniquePath = $"{newPath} ({counter++})";
            Directory.CreateDirectory(uniquePath);
            var folderName = Path.GetFileName(uniquePath);
            var newNode = new FolderNode
            {
                Name = folderName,
                OriginalName = folderName,
                FullPath = uniquePath,
                IsFolder = true,
                IsRenaming = true
            };
            parent.Children.Add(newNode);
            var tvi = FindTreeViewItem(FolderTreeView, parent);
            if (tvi != null)
                tvi.IsExpanded = true;

            await Task.Delay(20);

            var newTvi = FindTreeViewItem(FolderTreeView, newNode);
            var textBox = FindChild<TextBox>(newTvi);
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Deletes the selected folder or file from disk after user confirmation.
        /// The FileSystemWatcher handles the tree update automatically.
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem mi || mi.Tag is not FolderNode node) return;
            var message = node.IsFolder
                ? $"Are you sure you want to delete '{node.Name}' and all its contents?"
                : $"Are you sure you want to delete '{node.Name}'?";
            var result = MessageBox.Show(message, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                if (node.IsFolder)
                    Directory.Delete(node.FullPath, recursive: true);
                else
                    File.Delete(node.FullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Walks up the visual tree to find the nearest parent of type T.
        /// </summary>
        private T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T match) return match;
            return FindParent<T>(parent);
        }

        /// <summary>
        /// Recursively searches an ItemsControl to find the TreeViewItem
        /// that corresponds to a given FolderNode.
        /// </summary>
        private TreeViewItem? FindTreeViewItem(ItemsControl container, FolderNode node)
        {
            foreach (var item in container.Items)
            {
                var tvi = container.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (tvi == null) continue;
                if (item == node) return tvi;
                var found = FindTreeViewItem(tvi, node);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Recursively searches the visual tree to find the first child element of type T.
        /// </summary>
        private T? FindChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;
                var result = FindChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>
        /// Starts a drag operation when the user holds the mouse button and moves on a tree item.
        /// </summary>
        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (sender is not Border border) return;
            if (border.DataContext is not FolderNode node) return;

            _draggedNode = node;
            DragDrop.DoDragDrop(border, node, DragDropEffects.Move | DragDropEffects.Copy);
        }

        /// <summary>
        /// Validates the drag over a target item — only allows dropping onto folders.
        /// </summary>
        private void Header_DragOver(object sender, DragEventArgs e)
        {
            // Explorer drop — let FolderTree_DragOver handle it
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = false;
                return;
            }

            if (sender is not Border border) return;
            if (border.DataContext is not FolderNode target) return;

            if (_draggedNode == null || _draggedNode == target || !target.IsFolder)
                e.Effects = DragDropEffects.None;
            else
                e.Effects = DragDropEffects.Move;

            e.Handled = true;
        }

        /// <summary>
        /// Handles in-app drop by delegating to HandleDrop with move semantics.
        /// </summary>
        private void Header_Drop(object sender, DragEventArgs e)
        {
            // Explorer drop — let FolderTree_Drop handle it
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = false;
                return;
            }

            if (sender is not Border border) return;
            if (border.DataContext is not FolderNode target || !target.IsFolder) return;
            if (_draggedNode == null || _draggedNode == target) return;

            HandleDrop(new[] { _draggedNode.FullPath }, target.FullPath, copy: false);
            _draggedNode = null;
        }

        private void FolderTree_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var validExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".m4a" };
                bool hasAudio = files.Any(f => validExtensions.Contains(
                    Path.GetExtension(f).ToLower()));
                e.Effects = hasAudio ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else if (_draggedNode != null)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void FolderTree_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var targetFolder = GetDropTargetFolder(e);
            HandleDrop(files, targetFolder, copy: true);
        }

        /// <summary>
        /// Determines which folder the user dropped onto.
        /// Falls back to the library root if no folder node is found.
        /// </summary>
        private string GetDropTargetFolder(DragEventArgs e)
        {
            var element = e.OriginalSource as UIElement;
            var border = FindParent<Border>(element);

            if (border?.DataContext is FolderNode node)
                return node.IsFolder ? node.FullPath : Path.GetDirectoryName(node.FullPath)!;

            // Fallback to library root
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Soundbard", "Audio");
        }
        /// <summary>
        /// Shared logic for handling any drop onto the tree.
        /// copies from Explorer, moves from in-app drag.
        /// </summary>
        private void HandleDrop(string[] sourcePaths, string targetFolder, bool copy)
        {
            var validExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".m4a" };

            foreach (var sourcePath in sourcePaths)
            {
                var ext = Path.GetExtension(sourcePath).ToLower();
                var isFolder = Directory.Exists(sourcePath);

                if (!isFolder && !validExtensions.Contains(ext)) continue;

                var fileName = Path.GetFileName(sourcePath);
                var destination = Path.Combine(targetFolder, fileName);

                // Prevent dropping into itself
                if (isFolder && destination.StartsWith(sourcePath + Path.DirectorySeparatorChar))
                {
                    MessageBox.Show("Cannot move a folder into its own subfolder.");
                    continue;
                }

                // Handle conflicts
                bool exists = isFolder ? Directory.Exists(destination) : File.Exists(destination);
                if (exists && sourcePath != destination)
                {
                    var result = MessageBox.Show(
                        $"'{fileName}' already exists in that folder. Replace it?",
                        "Conflict", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes) continue;
                }

                try
                {
                    if (copy)
                    {
                        if (isFolder)
                            CopyDirectory(sourcePath, destination);
                        else
                            File.Copy(sourcePath, destination, overwrite: true);
                    }
                    else
                    {
                        if (isFolder)
                            Directory.Move(sourcePath, destination);
                        else
                            File.Move(sourcePath, destination, overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not move '{fileName}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Recursively copies a directory and its contents.
        /// Needed because Directory has no built-in Copy method.
        /// </summary>
        private void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)));
        }
    }
}