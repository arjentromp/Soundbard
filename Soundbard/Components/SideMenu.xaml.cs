using Soundbard.ViewModels;
using Soundbard.Views;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Soundbard.Components
{
    /// <summary>
    /// Interaction logic for SideMenu.xaml
    /// </summary>
    public partial class SideMenu : UserControl
    {
        public SideMenu()
        {
            InitializeComponent();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not WorkspaceViewModel workspace)
            {
                MessageBox.Show(
                    "The workspace is unavailable.",
                    "Load Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                string profilesDirectory = workspace.ProfilesDirectory;
                Directory.CreateDirectory(profilesDirectory);

                var dialog = new OpenFileDialog
                {
                    Title = "Load Track Profile",
                    InitialDirectory = profilesDirectory,
                    Filter = "JSON profile (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = ".json",
                    CheckFileExists = true,
                    Multiselect = false,
                    RestoreDirectory = true
                };

                if (dialog.ShowDialog(Application.Current.MainWindow) != true)
                    return;

                int loadedCount = workspace.LoadTracks(dialog.FileName);
                MessageBox.Show(
                    $"Loaded {loadedCount} track(s).",
                    "Load Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException or JsonException or NotSupportedException or ArgumentException)
            {
                MessageBox.Show(
                    $"Failed to load profile:\n{ex.Message}",
                    "Load Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not WorkspaceViewModel workspace)
            {
                MessageBox.Show(
                    "The workspace is unavailable.",
                    "Save Tracks",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var dialog = new SaveFileNameDialog
            {
                Owner = Application.Current.MainWindow
            };
            if (dialog.ShowDialog() != true || dialog.FileName == null)
                return;

            try
            {
                string filePath = workspace.SaveTracks(dialog.FileName);
                MessageBox.Show(
                    $"Tracks saved to:\n{filePath}",
                    "Save Tracks",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException or JsonException or NotSupportedException or ArgumentException)
            {
                MessageBox.Show(
                    $"Failed to save tracks:\n{ex.Message}",
                    "Save Tracks",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
