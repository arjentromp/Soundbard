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

        //Constructor---------------------------------------------------------------------------------
        public FolderTreeViewModel()
        {
            EnsureLibraryExists();
            LoadTree();
        }

        //Methods-------------------------------------------------------------------------------------

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
        private void LoadTree()
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

            foreach (var dir in Directory.GetDirectories(path))
                node.Children.Add(BuildNode(dir));

            foreach (var file in Directory.GetFiles(path)
                .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLower())))
            {
                node.Children.Add(new FolderNode
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsFolder = false
                });
            }

            return node;
        }
    }
}
