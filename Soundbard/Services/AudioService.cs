using Soundbard.Models;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.IO;
using System.Windows.Media;

namespace Soundbard.Services
{
    /// <summary>
    /// Handles all audio playback logic for a single track.
    /// Each TrackViewModel owns one instance of this service.
    /// </summary>
    public class AudioService
    {
        private readonly MediaPlayer _player;
        private readonly Track _track;
        private readonly ObservableCollection<string> _filePaths;
        private readonly Random _random = new Random();
        private bool _isPlaying;

        public event Action<bool>? PlayingChanged;
        public event Action<string>? FileChanged;

        public AudioService(Track track)
        {
            _player = new MediaPlayer();
            _track = track;
            _filePaths = track.FilePaths;

            _player.MediaEnded += OnMediaEnded;
        }

        /// <summary>
        /// Loads an audio file for the MediaPlayer
        /// </summary>
        private string? _currentFile;

        public void Load(string filePath)
        {
            if (_currentFile == filePath) return;

            _player.Open(new Uri(filePath));
            _currentFile = filePath;

            FileChanged?.Invoke(Path.GetFileNameWithoutExtension(filePath));
        }

        private int _currentIndex = -1;

        /// <summary>
        /// Checks what the QueueMode is set to and picks a file based on it.
        /// </summary>
        private string? GetNextFile()
        {
            if (_filePaths.Count == 0) return null;

            if (_track.QueueMode == QueueMode.Sequential)
            {
                _currentIndex++;

                if (_currentIndex >= _filePaths.Count)
                {
                    _currentIndex = 0;
                }

                return _filePaths[_currentIndex];
            }
            else
            {
                int index;
                if (_filePaths.Count == 1)
                {
                    return _filePaths[0];
                }

                do
                {
                    index = _random.Next(0, _filePaths.Count);
                }
                while (index == _currentIndex);

                _currentIndex = index;

                return _filePaths[index];
            }
        }

        /// <summary>
        /// Checks if a file is loaded, if not, it loads the next file.
        /// </summary>
        private void EnsureFileLoaded()
        {
            if (_currentFile == null && _filePaths.Count > 0)
            {
                var next = GetNextFile();
                if (next != null)
                {
                    Load(next);
                }
            }
        }

        /// <summary>
        /// Toggles between playing and pausing.
        /// </summary>
        public void PlayPause()
        {
            EnsureFileLoaded();

            if (_currentFile == null) return;

            if (_isPlaying)
            {
                _player.Pause();
                _isPlaying = false;
                PlayingChanged?.Invoke(false);
            }
            else
            {
                _player.Play();
                _isPlaying = true;
                PlayingChanged?.Invoke(true);
            }
        }

        /// <summary>
        /// Sets the volume to the slider.
        /// </summary>
        public void SetVolume(int volume)
        {
            volume = Math.Max(0, Math.Min(100, volume));

            _player.Volume = volume / 100.0;
        }

        /// <summary>
        /// Stops playback entirely and resets position.
        /// </summary>
        public void Stop()
        {
            _player.Stop();
            HandlePlaybackStopped();
        }

        /// <summary>
        /// When OnMediaEnded event fires, decide which action to take
        /// </summary>
        private void OnMediaEnded(object? sender, EventArgs e)
        {
            switch (_track.PlayMode)
            {
                case PlayMode.Continuous:
                    var nextContinuous = GetNextFile();
                    if (nextContinuous != null)
                    {
                        Load(nextContinuous);
                        _player.Play();

                        _isPlaying = true;
                        PlayingChanged?.Invoke(true);
                    }
                    else
                    {
                        HandlePlaybackStopped();
                    }
                    break;

                case PlayMode.Manual:
                    HandlePlaybackStopped();

                    var nextManual = GetNextFile();
                    if (nextManual != null)
                    {
                        Load(nextManual);
                    }


                    break;

                case PlayMode.Interval:
                    // future logic
                    HandlePlaybackStopped();
                    break;
            }
        }

        /// <summary>
        /// Sets the track IsPlaying to false
        /// </summary>
        private void HandlePlaybackStopped()
        {
            _player.Stop();
            _isPlaying = false;
            PlayingChanged?.Invoke(false);
        }
    }
}