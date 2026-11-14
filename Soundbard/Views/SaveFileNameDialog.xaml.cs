using System.IO;
using System.Windows;

namespace Soundbard.Views
{
    public partial class SaveFileNameDialog : Window
    {
        public string? FileName { get; private set; }

        public SaveFileNameDialog()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                FileNameBox.Focus();
                FileNameBox.SelectAll();
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FileNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                fileName is "." or "..")
            {
                ValidationMessage.Text = "Enter a valid file name without folder paths.";
                ValidationMessage.Visibility = Visibility.Visible;
                return;
            }

            FileName = fileName.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase)
                ? fileName[..^5]
                : fileName;
            if (string.IsNullOrWhiteSpace(FileName) || FileName is "." or "..")
            {
                ValidationMessage.Text = "Enter a valid file name.";
                ValidationMessage.Visibility = Visibility.Visible;
                return;
            }

            DialogResult = true;
        }
    }
}
