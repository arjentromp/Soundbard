using Soundbard.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace Soundbard.Services
{
    public class AudioService
    {
        private MediaPlayer _currentPlayer;
        private MediaPlayer _nextPlayer;
        private DispatcherTimer? _fadeTimer;

        private readonly Track _track;
        private readonly ObservableCollection<string> _filePaths;
        private readonly Random _random = new Random();
        private DispatcherTimer? _intervalTimer;
        private DateTime _nextTriggerTime;

        private int _currentIndex = -1;
        private string? _currentFile;
        private bool _isPlaying;
        private double _fadeProgress;

        public event Action<bool>? PlayingChanged;
        public event Action<string>? FileChanged;

        public AudioService(Track track)
        {
            _track = track;
            _filePaths = track.FilePaths;

            _currentPlayer = new MediaPlayer();
            _nextPlayer = new MediaPlayer();

            _currentPlayer.MediaEnded += OnMediaEnded;
        }

        /// <summary>
        /// Loads a file into a specific player.
        /// Only updates UI when loading into current player.
        /// </summary>
        private void Load(MediaPlayer player, string filePath)
        {
            player.Open(new Uri(filePath));

            if (player == _currentPlayer)
            {
                _currentFile = filePath;
                FileChanged?.Invoke(Path.GetFileNameWithoutExtension(filePath));
            }
        }

        /// <summary>
        /// Returns the next file AND advances the index.
        /// This is ONLY used by Preload().
        /// </summary>
        private string? GetNextFile()
        {
            if (_filePaths.Count == 0) return null;

            if (_track.QueueMode == QueueMode.Sequential)
            {
                _currentIndex++;

                if (_currentIndex >= _filePaths.Count)
                    _currentIndex = 0;

                return _filePaths[_currentIndex];
            }
            else
            {
                if (_filePaths.Count == 1)
                    return _filePaths[0];

                int index;
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
        /// Preloads the next file into _nextPlayer.
        /// This is the ONLY place where GetNextFile is called.
        /// </summary>
        private void PreloadNext()
        {
            var next = GetNextFile();

            if (next != null)
            {
                Load(_nextPlayer, next);
            }
        }

        /// <summary>
        /// Swaps current and next players.
        /// Also reassigns MediaEnded event correctly.
        /// </summary>
        private void SwapPlayers()
        {
            _currentPlayer.MediaEnded -= OnMediaEnded;

            var temp = _currentPlayer;
            _currentPlayer = _nextPlayer;
            _nextPlayer = temp;

            _currentPlayer.MediaEnded += OnMediaEnded;
        }

        /// <summary>
        /// Ensures first track is loaded and next track is preloaded.
        /// </summary>
        private void InitializePlayback()
        {
            if (_currentFile != null || _filePaths.Count == 0)
                return;

            var first = GetNextFile();
            if (first == null) return;

            Load(_currentPlayer, first);

            PreloadNext();
        }

        /// <summary>
        /// Toggles play/pause.
        /// </summary>
        public void PlayPause()
        {
            InitializePlayback();

            if (_currentFile == null) return;

            if (_isPlaying)
            {
                _currentPlayer.Pause();
                _isPlaying = false;
                PlayingChanged?.Invoke(false);
                StopInterval();
            }
            else
            {
                _currentPlayer.Play();
                _isPlaying = true;
                PlayingChanged?.Invoke(true);
                if (_track.PlayMode == PlayMode.Interval)
                {
                    StartInterval();
                }
            }
        }

        /// <summary>
        /// Handles what happens when a track finishes.
        /// </summary>
        private void OnMediaEnded(object? sender, EventArgs e)
        {
            switch (_track.PlayMode)
            {
                case PlayMode.Continuous:
                    PlayNextInternal(autoPlay: true);
                    break;

                case PlayMode.Manual:
                    PlayNextInternal(autoPlay: false);
                    break;

                case PlayMode.Interval:
                    break;
            }
        }

        /// <summary>
        /// Plays or sets up the next file depending on Continious mode or Manual
        /// </summary>
        private void PlayNextInternal(bool autoPlay)
        {
            SwapPlayers();

            if (_currentIndex >= 0 && _currentIndex < _filePaths.Count)
            {
                _currentFile = _filePaths[_currentIndex];
                FileChanged?.Invoke(Path.GetFileNameWithoutExtension(_currentFile));
            }

            if (autoPlay)
            {
                _currentPlayer.Play();
                _isPlaying = true;
                PlayingChanged?.Invoke(true);
            }
            else
            {
                HandlePlaybackStopped();
            }

            PreloadNext();
        }

        /// <summary>
        /// Plays next audio file for interval mode
        /// </summary>
        private void PlayNextInterval()
        {
            SwapPlayers();
            PreloadNext();

            if (_currentIndex >= 0 && _currentIndex < _filePaths.Count)
            {
                _currentFile = _filePaths[_currentIndex];
                FileChanged?.Invoke(Path.GetFileNameWithoutExtension(_currentFile));
            }

            _currentPlayer.Play();
        }

        /// <summary>
        /// Interval timer event
        /// </summary>
        private void OnIntervalTick(object? sender, EventArgs e)
        {
            _intervalTimer?.Stop();

            PlayNextInterval();

            StartInterval();
        }

        /// <summary>
        /// Calculate next interval using the _track values
        /// </summary>
        private TimeSpan GetNextInterval()
        {
            int baseSeconds = _track.IntervalSeconds;
            int deviation = _track.IntervalDeviation;

            int offset;

            if (deviation > 0)
            {
                offset = _random.Next(-deviation, deviation + 1);
            }
            else
            {
                offset = 0;
            }

            int finalSeconds = Math.Max(0, baseSeconds + offset);

            return TimeSpan.FromSeconds(finalSeconds);
        }

        /// <summary>
        /// Starts the interval timer
        /// </summary>
        private void StartInterval()
        {
            StopInterval();

            var interval = GetNextInterval();
            _nextTriggerTime = DateTime.Now.Add(interval);

            _intervalTimer = new DispatcherTimer
            {
                Interval = interval
            };

            _intervalTimer.Tick += OnIntervalTick;
            _intervalTimer.Start();
        }

        /// <summary>
        /// Stops the interval timer
        /// </summary>
        private void StopInterval()
        {
            if (_intervalTimer == null) return;

            _intervalTimer.Stop();
            _intervalTimer.Tick -= OnIntervalTick;
            _intervalTimer = null;
        }

        /// <summary>
        /// Sets the volume of _currentPlayer
        /// </summary>
        public void SetVolume(int volume)
        {
            volume = Math.Max(0, Math.Min(100, volume));
            _currentPlayer.Volume = volume / 100.0;
        }

        /// <summary>
        /// Stops playback on _currentPlayer
        /// </summary>
        public void Stop()
        {
            StopInterval();
            _currentPlayer.Stop();
            HandlePlaybackStopped();
        }

        /// <summary>
        /// Sets _isPlaying to false and invokes playing changed event
        /// </summary>
        private void HandlePlaybackStopped()
        {
            _isPlaying = false;
            PlayingChanged?.Invoke(false);
        }

        /// <summary>
        /// Nukes the Audio services and timers.
        /// </summary>
        public void Dispose()
        {
            _currentPlayer.MediaEnded -= OnMediaEnded;

            _currentPlayer.Stop();
            _currentPlayer.Close();

            _nextPlayer.Stop();
            _nextPlayer.Close();
        }
    }
}