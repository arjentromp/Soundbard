using Soundbard.Components;
using Soundbard.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Soundbard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.StateChanged += Window_StateChanged;
            this.PreviewKeyDown += Window_PreviewKeyDown;
        }

        /// <summary>
        /// Changes the margin when fullscreen to prevent cutting off the edges
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainGrid.Margin = new Thickness(7);
            }
            else
            {
                MainGrid.Margin = new Thickness(0);
            }
        }

        /// <summary>
        /// Makes the entire page draggable
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// Handles hotkey presses: plays every track whose Hotkey matches the pressed key.
        /// Only fires when the window has focus (PreviewKeyDown guarantees this).
        /// </summary>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ignore modifier-only keypresses
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LWin || e.Key == Key.RWin)
                return;

            string pressedKey = e.Key.ToString(); // e.g. "A", "F1", "D1"

            var workspace = FindVisualChild<Workspace>(this);
            if (workspace?.DataContext is not WorkspaceViewModel vm)
                return;

            bool handled = false;
            foreach (var item in vm.Items)
            {
                if (item is TrackViewModel track &&
                    !string.IsNullOrWhiteSpace(track.Hotkey) &&
                    string.Equals(track.Hotkey, pressedKey, StringComparison.OrdinalIgnoreCase))
                {
                    track.PlayPauseCommand.Execute(null);
                    handled = true;
                }
            }

            if (handled)
                e.Handled = true;
        }

        /// <summary>
        /// Walks the visual tree to find the first child of type T.
        /// </summary>
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match)
                    return match;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}