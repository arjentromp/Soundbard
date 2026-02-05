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

namespace Soundbard
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // MainWindow.xaml.cs
        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem is TreeViewItem selected)
            {
                MessageBox.Show($"Rename clicked on: {selected.Header}");
                // Later: replace Header with a TextBox for inline editing
            }
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem is TreeViewItem selected)
            {
                var newFolder = new TreeViewItem() { Header = "New Folder" };
                selected.Items.Add(newFolder);
                selected.IsExpanded = true;
            }
        }

    }
}