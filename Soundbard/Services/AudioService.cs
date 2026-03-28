using Soundbard.Models;
using System;
using System.Collections.ObjectModel;
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
        private readonly ObservableCollection<string> _filePaths;
        private bool _isPlaying;

        /// <summary>
        /// Initializes the AudioService with the track's file list.
        /// </summary>
        public AudioService(ObservableCollection<string> filePaths)
        {
            _player = new MediaPlayer();
            _filePaths = filePaths;
        }

        /// <summary>
        /// Toggles between playing and pausing. Plays the first file if nothing is loaded yet.
        /// </summary>
        public void PlayPause()
        {
            if (_filePaths.Count == 0) return;

            if (_isPlaying)
            {
                _player.Pause();
                _isPlaying = false;
            }
            else
            {
                _player.Open(new Uri(_filePaths[0]));
                _player.Play();
                _isPlaying = true;
            }
        }

        /// <summary>
        /// Stops playback entirely and resets position.
        /// </summary>
        public void Stop()
        {
            _player.Stop();
            _isPlaying = false;
        }
    }
}