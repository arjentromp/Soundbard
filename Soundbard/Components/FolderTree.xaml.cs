using Soundbard.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Soundbard.Components
{
    /// <summary>
    /// Interaction logic for FolderTree.xaml
    /// </summary>
    public partial class FolderTree : UserControl
    {
        public FolderTree()
        {
            InitializeComponent();
            DataContext = new FolderTreeViewModel();
        }
        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var tvi = border?.TemplatedParent as TreeViewItem
                      ?? FindParent<TreeViewItem>(border);
            if (tvi != null)
                tvi.IsExpanded = !tvi.IsExpanded;
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T match) return match;
            return FindParent<T>(parent);
        }
    }
}
