using Soundbard.Commands;
using Soundbard.Models;
using Soundbard.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Soundbard.ViewModels
{
    public class TrackViewModel : BaseViewModel
    {
        private readonly Track _track;
        private readonly WorkspaceViewModel _workspace;
        private readonly AudioService _audioService;

        /// <summary>
        /// Initializes the TrackViewModel with its underlying model, parent workspace,
        /// and a new AudioService instance for handling playback.
        /// </summary>
        public TrackViewModel(Track track, WorkspaceViewModel workspace)
        {
            _track = track;
            _workspace = workspace;

            _audioService = new AudioService(_track);

            _audioService.PlayingChanged += isPlaying => IsPlaying = isPlaying;
            _audioService.FileChanged += fileName => CurrentFileName = fileName;
        }

        // --- Track properties ---

        /// <summary>
        /// The display name of the track.
        /// </summary>
        public string Name
        {
            get => _track.Name;
            set { _track.Name = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The keyboard hotkey assigned to this track.
        /// </summary>
        public string Hotkey
        {
            get => _track.Hotkey;
            set { _track.Hotkey = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The border color of the track card.
        /// </summary>
        public Color BorderColor
        {
            get => _track.BorderColor;
            set { _track.BorderColor = value; OnPropertyChanged(); OnPropertyChanged(nameof(BorderBrush)); }
        }

        /// <summary>
        /// A SolidColorBrush derived from BorderColor, used for XAML binding.
        /// </summary>
        public SolidColorBrush BorderBrush => new SolidColorBrush(_track.BorderColor);

        /// <summary>
        /// The volume level of the track (0-100).
        /// </summary>
        public int Volume
        {
            get => _track.Volume;
            set
            {
                _track.Volume = value;
                OnPropertyChanged();

                _audioService.SetVolume(value);
            }
        }

        /// <summary>
        /// The list of file paths associated with this track.
        /// </summary>
        public ObservableCollection<string> FilePaths => _track.FilePaths;

        // --- Playback settings ---

        /// <summary>
        /// Determines how the track behaves when playing (Continuous, Manual, Interval).
        /// </summary>
        public PlayMode PlayMode
        {
            get => _track.PlayMode;
            set { _track.PlayMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        /// <summary>
        /// Determines the order files are played (Shuffle, Sequential).
        /// </summary>
        public QueueMode QueueMode
        {
            get => _track.QueueMode;
            set { _track.QueueMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        /// <summary>
        /// Whether fade in/out is enabled for this track.
        /// </summary>
        public bool FadeEnabled
        {
            get => _track.FadeEnabled;
            set { _track.FadeEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        /// <summary>
        /// The duration of the fade effect in seconds.
        /// </summary>
        public int FadeDuration
        {
            get => _track.FadeDuration;
            set { _track.FadeDuration = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        /// <summary>
        /// The base interval duration in seconds between plays.
        /// </summary>
        public int IntervalSeconds
        {
            get => _track.IntervalSeconds;
            set { _track.IntervalSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        /// <summary>
        /// The maximum random deviation added to the interval in seconds.
        /// </summary>
        public int IntervalDeviation
        {
            get => _track.IntervalDeviation;
            set { _track.IntervalDeviation = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveTags)); }
        }

        // --- Runtime state ---

        /// <summary>
        /// Whether the track is currently playing. Not persisted.
        /// </summary>
        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The filename currently being played. Not persisted.
        /// </summary>
        private string _currentFileName = string.Empty;
        public string CurrentFileName
        {
            get => _currentFileName;
            set { _currentFileName = value; OnPropertyChanged(); }
        }

        // --- Computed properties ---

        /// <summary>
        /// A collection of active setting tags displayed on the track card.
        /// </summary>
        public ObservableCollection<string> ActiveTags
        {
            get
            {
                var tags = new ObservableCollection<string>();
                tags.Add(PlayMode.ToString());
                tags.Add(QueueMode.ToString());
                if (FadeEnabled) tags.Add($"Fade {FadeDuration}s");
                if (PlayMode == PlayMode.Interval)
                {
                    tags.Add($"Interval {IntervalSeconds}s");
                    tags.Add($"Deviation {IntervalDeviation}s");
                }
                return tags;
            }
        }

        // --- Commands ---

        /// <summary>
        /// Removes this track from the workspace and disposes of its child MediaPlayer
        /// </summary>
        public ICommand DeleteCommand => new RelayCommand(() =>
        {
            DisposeAudioService();
            _workspace.DeleteTrack(this);
        });

        public void DisposeAudioService() => _audioService.Dispose();

        /// <summary>
        /// Toggles playback on this track between playing and paused.
        /// </summary>
        public ICommand PlayPauseCommand => new RelayCommand(() => _audioService.PlayPause());

        // --- File management ---

        /// <summary>
        /// Moves the specified file one position up in the FilePaths list.
        /// </summary>
        public void MoveUp(string filePath)
        {
            var index = FilePaths.IndexOf(filePath);
            if (index <= 0) return;
            FilePaths.Move(index, index - 1);
        }

        /// <summary>
        /// Moves the specified file one position down in the FilePaths list.
        /// </summary>
        public void MoveDown(string filePath)
        {
            var index = FilePaths.IndexOf(filePath);
            if (index < 0 || index >= FilePaths.Count - 1) return;
            FilePaths.Move(index, index + 1);
        }

        /// <summary>
        /// Removes the specified file from the FilePaths list.
        /// </summary>
        public void RemoveFile(string filePath)
        {
            FilePaths.Remove(filePath);
        }

        // --- Model access ---

        /// <summary>
        /// Returns the underlying Track model, used for serialization.
        /// </summary>
        public Track GetModel() => _track;
    }
}