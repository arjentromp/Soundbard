using Soundbard.Models;
using Soundbard.ViewModels;
using Soundbard.Views;
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
using System.IO;

namespace Soundbard.Components
{
    /// <summary>
    /// Interaction logic for TrackCard.xaml
    /// </summary>
    public partial class TrackCard : UserControl
    {
        public TrackCard()
        {
            InitializeComponent();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TrackViewModel vm) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{vm.Name}'?",
                "Delete Track",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                vm.DeleteCommand.Execute(null);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TrackViewModel vm) return;

            var dialog = new TrackOptionsDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.LoadFromViewModel(vm);

            // Pre-fill with current values
            dialog.TrackNameBox.Text = vm.Name;

            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                vm.Name = dialog.Result.Name;
                vm.Hotkey = dialog.Result.Hotkey;
                vm.BorderColor = dialog.Result.BorderColor;
                vm.PlayMode = dialog.Result.PlayMode;
                vm.QueueMode = dialog.Result.QueueMode;
                vm.FadeEnabled = dialog.Result.FadeEnabled;
                vm.FadeDuration = dialog.Result.FadeDuration;
                vm.IntervalSeconds = dialog.Result.IntervalSeconds;
                vm.IntervalDeviation = dialog.Result.IntervalDeviation;
            }
        }

        /// <summary>
        /// Opens the Manage audio window
        /// </summary>
        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TrackViewModel vm) return;

            var dialog = new ManageAudioDialog(vm);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        private void TrackCard_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(FolderNode)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void TrackCard_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is not TrackViewModel vm) return;

            var validExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".m4a" };

            if (e.Data.GetData(typeof(FolderNode)) is not FolderNode node) return;

            if (node.IsFolder)
            {
                foreach (var child in node.Children)
                {
                    if (!child.IsFolder &&
                        validExtensions.Contains(Path.GetExtension(child.FullPath).ToLower()) &&
                        !vm.FilePaths.Contains(child.FullPath))
                    {
                        vm.FilePaths.Add(child.FullPath);
                    }
                }
            }
            else
            {
                if (validExtensions.Contains(Path.GetExtension(node.FullPath).ToLower()) &&
                    !vm.FilePaths.Contains(node.FullPath))
                {
                    vm.FilePaths.Add(node.FullPath);
                }
            }

            e.Handled = true;
        }
    }
}
