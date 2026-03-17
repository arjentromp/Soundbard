using System;
using System.Collections.Generic;
using System.Text;
using Soundbard.Models;
using Soundbard.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Soundbard.ViewModels
{
    public class TrackViewModel : BaseViewModel
    {
        private readonly Track _track;
        private readonly WorkspaceViewModel _workspace;

        public TrackViewModel(Track track, WorkspaceViewModel workspace)
        {
            _track = track;
            _workspace = workspace;
        }

        // --- Passthrough properties to the model ---

        public string Name
        {
            get => _track.Name;
            set { _track.Name = value; OnPropertyChanged(); }
        }

        public string Hotkey
        {
            get => _track.Hotkey;
            set { _track.Hotkey = value; OnPropertyChanged(); }
        }

        public Color BorderColor
        {
            get => _track.BorderColor;
            set { _track.BorderColor = value; OnPropertyChanged(); OnPropertyChanged(nameof(BorderBrush)); }
        }

        public SolidColorBrush BorderBrush => new SolidColorBrush(_track.BorderColor);

        public PlayMode PlayMode
        {
            get => _track.PlayMode;
            set { _track.PlayMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public QueueMode QueueMode
        {
            get => _track.QueueMode;
            set { _track.QueueMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public bool FadeEnabled
        {
            get => _track.FadeEnabled;
            set { _track.FadeEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public int FadeDuration
        {
            get => _track.FadeDuration;
            set { _track.FadeDuration = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public bool IntervalEnabled
        {
            get => _track.IntervalEnabled;
            set { _track.IntervalEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public int IntervalSeconds
        {
            get => _track.IntervalSeconds;
            set { _track.IntervalSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public bool IntervalRandom
        {
            get => _track.IntervalRandom;
            set { _track.IntervalRandom = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public int IntervalDeviation
        {
            get => _track.IntervalDeviation;
            set { _track.IntervalDeviation = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        public int Volume
        {
            get => _track.Volume;
            set { _track.Volume = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> FilePaths => _track.FilePaths;

        // --- Runtime only properties, never saved ---

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        private string _currentFileName = string.Empty;
        public string CurrentFileName
        {
            get => _currentFileName;
            set { _currentFileName = value; OnPropertyChanged(); }
        }

        // --- Computed properties ---

        public ObservableCollection<string> ActiveTags
        {
            get
            {
                var tags = new ObservableCollection<string>();
                tags.Add(PlayMode.ToString());
                tags.Add(QueueMode.ToString());
                if (FadeEnabled) tags.Add($"Fade {FadeDuration}s");
                if (IntervalEnabled)
                {
                    tags.Add($"Interval {IntervalSeconds}s");
                    tags.Add($"Deviation {IntervalDeviation}s");
                }
                return tags;
            }
        }

        // --- Commands ---

        public ICommand DeleteCommand => new RelayCommand(() => _workspace.DeleteTrack(this));

        // --- Expose the underlying model for saving ---

        public Track GetModel() => _track;
    }
}