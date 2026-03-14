using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Soundbard.Models
{
    public class FolderNode
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public ObservableCollection<FolderNode> Children { get; set; } = new();
    }
}
