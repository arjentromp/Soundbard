using System;
using System.Collections.Generic;
using System.Text;
using Soundbard.Models;
using System.Collections.ObjectModel;

namespace Soundbard.ViewModels
{
    public class TrackViewModel : BaseViewModel
    {
        public PlayMode PlayMode { get; set; }
        public QueueMode QueueMode { get; set; }
        public bool FadeEnabled { get; set; }
        public int FadeDuration { get; set; }

        public ObservableCollection<string> ActiveTags
        {
            get
            {
                var tags = new ObservableCollection<string>();
                tags.Add(PlayMode.ToString());
                tags.Add(QueueMode.ToString());
                if (FadeEnabled) tags.Add($"Fade {FadeDuration}s");
                return tags;
            }
        }
    }
}
