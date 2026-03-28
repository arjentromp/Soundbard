using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Soundbard.Models
{
    public class Track
    {
        public string Name { get; set; } = string.Empty;
        public string Hotkey { get; set; } = string.Empty;
        public Color BorderColor { get; set; } = Colors.Red;

        public PlayMode PlayMode { get; set; } = PlayMode.Continuous;
        public QueueMode QueueMode { get; set; } = QueueMode.Shuffle;

        public bool FadeEnabled { get; set; } = false;
        public int FadeDuration { get; set; } = 2;

        public int IntervalSeconds { get; set; } = 30;
        public int IntervalDeviation { get; set; } = 10;

        public int Volume { get; set; } = 100;

        public ObservableCollection<string> FilePaths { get; set; } = new();
    }
}
