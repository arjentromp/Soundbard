using Soundbard.Models;
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
    public partial class TrackOptionsDialog : Window
    {
        public Track? Result { get; private set; }

        private string _hotkey = string.Empty;

        private Color _selectedColor = Color.FromRgb(0xc0, 0x39, 0x2b); // default red

        /// <summary>
        /// Initializes the TrackOptionsDialog window.
        /// </summary>
        public TrackOptionsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the dialog and discards any changes by setting DialogResult to false.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Focuses the hotkey TextBlock and highlights the border gold to indicate it is listening for input.
        /// </summary>
        private void HotkeyBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HotkeyDisplay.Focus();
            HotkeyBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xaf, 0x92, 0x41));
        }

        /// <summary>
        /// Captures the pressed key, displays it in the hotkey field, and resets the border colour.
        /// </summary>
        private void HotkeyDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            _hotkey = e.Key.ToString();
            HotkeyDisplay.Text = _hotkey;
            HotkeyDisplay.Foreground = Brushes.White;
            HotkeyBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3b, 0x37, 0x34));
        }

        /// <summary>
        /// Handles swatch selection by clearing all selection rings, highlighting the clicked swatch,
        /// and storing the selected colour for use when confirming.
        /// </summary>
        private void Swatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            foreach (Button swatch in SwatchPanel.Children)
                swatch.BorderBrush = Brushes.Transparent;

            btn.BorderBrush = Brushes.White;

            _selectedColor = (Color)ColorConverter.ConvertFromString(btn.Tag.ToString()!);
        }

        /// <summary>
        /// Collects all form values, constructs a Track object, assigns it to Result,
        /// and closes the dialog with DialogResult true to signal a successful creation.
        /// </summary>
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(FadeDurationBox.Text, out int fadeDuration);
            int.TryParse(IntervalDurationBox.Text, out int intervalSeconds);
            int.TryParse(IntervalDeviationBox.Text, out int intervalDeviation);

            Result = new Track
            {
                Name = string.IsNullOrWhiteSpace(TrackNameBox.Text) ? "New Track" : TrackNameBox.Text.Trim(),
                Hotkey = _hotkey,
                BorderColor = _selectedColor,
                PlayMode = PlayModeBox.SelectedIndex switch
                {
                    1 => PlayMode.Continuous,
                    2 => PlayMode.Manual,
                    3 => PlayMode.Interval,
                    _ => PlayMode.Continuous
                },
                QueueMode = QueueModeBox.SelectedIndex switch
                {
                    1 => QueueMode.Shuffle,
                    2 => QueueMode.Sequential,
                    _ => QueueMode.Shuffle
                },
                FadeEnabled = FadeEnabledBox.IsChecked == true,
                FadeDuration = fadeDuration > 0 ? fadeDuration : 2,
                IntervalSeconds = intervalSeconds > 0 ? intervalSeconds : 30,
                IntervalDeviation = intervalDeviation >= 0 ? intervalDeviation : 10,
                Volume = 75
            };

            DialogResult = true;
        }

        /// <summary>
        /// Accepts a viewmodel and passes the values so when a track is being edited the settings page shows the correct settings.
        /// </summary>
        public void LoadFromViewModel(TrackViewModel vm)
        {
            TrackNameBox.Text = vm.Name;
            HotkeyDisplay.Text = string.IsNullOrEmpty(vm.Hotkey) ? "Click to assign..." : vm.Hotkey;
            HotkeyDisplay.Foreground = string.IsNullOrEmpty(vm.Hotkey) ?
                new SolidColorBrush(Color.FromRgb(0x7a, 0x70, 0x60)) : Brushes.White;
            _hotkey = vm.Hotkey;
            _selectedColor = vm.BorderColor;

            // Highlight the matching swatch
            foreach (Button swatch in SwatchPanel.Children)
            {
                var swatchColor = (Color)ColorConverter.ConvertFromString(swatch.Tag.ToString()!);
                swatch.BorderBrush = swatchColor == vm.BorderColor ? Brushes.White : Brushes.Transparent;
            }

            // Set play mode
            PlayModeBox.SelectedIndex = vm.PlayMode switch
            {
                PlayMode.Continuous => 1,
                PlayMode.Manual => 2,
                PlayMode.Interval => 3,
                _ => 0
            };

            // Set queue mode
            QueueModeBox.SelectedIndex = vm.QueueMode switch
            {
                QueueMode.Shuffle => 1,
                QueueMode.Sequential => 2,
                _ => 0
            };

            FadeEnabledBox.IsChecked = vm.FadeEnabled;
            FadeDurationBox.Text = vm.FadeDuration.ToString();
            IntervalDurationBox.Text = vm.IntervalSeconds.ToString();
            IntervalDeviationBox.Text = vm.IntervalDeviation.ToString();
        }
    }
}