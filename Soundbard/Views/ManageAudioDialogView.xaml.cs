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
using System.Windows.Shapes;

namespace Soundbard.Views
{
    /// <summary>
    /// Interaction logic for ManageAudioDialogView.xaml
    /// </summary>
    public partial class ManageAudioDialog : Window
    {
        public ManageAudioDialog(TrackViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        /// <summary>
        /// Closes the manage audio window
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Redirects the moveup click to the viewmodel
        /// </summary>
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string filePath)
                (DataContext as TrackViewModel)?.MoveUp(filePath);
        }

        /// <summary>
        /// Redirects the movedown click to the viewmodel
        /// </summary>
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string filePath)
                (DataContext as TrackViewModel)?.MoveDown(filePath);
        }

        /// <summary>
        /// Redirects the delete click to the viewmodel
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string filePath)
                (DataContext as TrackViewModel)?.RemoveFile(filePath);
        }
    }
}
