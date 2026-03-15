using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using Soundbard.Models;

namespace Soundbard.ViewModels
{
    public class FolderTreeViewModel : BaseViewModel
    {
        //Props---------------------------------------------------------------------------------------
        private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".m4a" };

        public ObservableCollection<FolderNode> RootNodes { get; } = new();

        private FileSystemWatcher? _watcher;

        //Constructor---------------------------------------------------------------------------------
        public FolderTreeViewModel()
        {
            EnsureLibraryExists();
            LoadTree();
            StartWatcher();
        }

        //Methods-------------------------------------------------------------------------------------
        /// <summary>
        /// Start file system watcher to maintain 1:1 between whats on the disk and whats in the app
        /// </summary>
        private void StartWatcher()
        {
            _watcher = new FileSystemWatcher(GetLibraryPath())
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            _watcher.Created += OnFileSystemChanged;
            _watcher.Deleted += OnFileSystemChanged;
            _watcher.Renamed += OnFileSystemChanged;
        }

        /// <summary>
        /// Reload tree when a change was detected inside of the audio folder
        /// </summary>
        private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (IsAnyNodeRenaming(RootNodes)) return;
                var expandedPaths = GetExpandedPaths();
                LoadTree();
                RestoreExpandedPaths(expandedPaths);
            });
        }

        private bool IsAnyNodeRenaming(IEnumerable<FolderNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.IsRenaming) return true;
                if (IsAnyNodeRenaming(node.Children)) return true;
            }
            return false;
        }

        private HashSet<string> GetExpandedPaths()
        {
            var paths = new HashSet<string>();
            CollectExpandedPaths(RootNodes, paths);
            return paths;
        }

        private void CollectExpandedPaths(IEnumerable<FolderNode> nodes, HashSet<string> paths)
        {
            foreach (var node in nodes)
            {
                if (node.IsExpanded)
                    paths.Add(node.FullPath);
                CollectExpandedPaths(node.Children, paths);
            }
        }

        private void RestoreExpandedPaths(HashSet<string> paths)
        {
            RestoreExpanded(RootNodes, paths);
        }

        private void RestoreExpanded(IEnumerable<FolderNode> nodes, HashSet<string> paths)
        {
            foreach (var node in nodes)
            {
                if (paths.Contains(node.FullPath))
                    node.IsExpanded = true;
                RestoreExpanded(node.Children, paths);
            }
        }

        /// <summary>
        /// Returns the root library path.
        /// </summary>
        private string GetLibraryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Soundbard", "Audio");
        }

        /// <summary>
        /// Generates the root folder if it does not yet exist.
        /// </summary>
        private void EnsureLibraryExists()
        {
            Directory.CreateDirectory(GetLibraryPath());
        }

        /// <summary>
        /// Loads the folder tree from the disk.
        /// </summary>
        public void LoadTree()
        {
            RootNodes.Clear();
            var root = BuildNode(GetLibraryPath());
            RootNodes.Add(root);
        }

        /// <summary>
        /// Builds nodes for every folder and file within the root folder.
        /// </summary>
        private FolderNode BuildNode(string path)
        {
            var node = new FolderNode
            {
                Name = Path.GetFileName(path),
                FullPath = path,
                IsFolder = true
            };

            foreach (var file in Directory.GetFiles(path).Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLower())))
            {
                node.Children.Add(new FolderNode
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    OriginalName = Path.GetFileNameWithoutExtension(file),
                    FullPath = file,
                    IsFolder = false
                });
            }

            foreach (var dir in Directory.GetDirectories(path))
                node.Children.Add(BuildNode(dir));

            return node;
        }
    }
}
