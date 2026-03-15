using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Soundbard.ViewModels;

namespace Soundbard.Models
{
    public class FolderNode : BaseViewModel
    {
        private string _name = string.Empty;
        private bool _isRenaming;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }
        public string OriginalName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public bool IsRenaming
        {
            get => _isRenaming;
            set { _isRenaming = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FolderNode> Children { get; set; } = new();
    }
}