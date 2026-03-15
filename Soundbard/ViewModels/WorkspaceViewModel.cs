using System;
using System.Collections.Generic;
using System.Text;
using Soundbard.Commands;
using Soundbard.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Soundbard.ViewModels
{
    public class WorkspaceViewModel : BaseViewModel
    {
        private readonly ObservableCollection<object> _items = new();
        public ObservableCollection<object> Items => _items;

        public ICommand AddTrackCommand => new RelayCommand(OpenAddTrackDialog);

        public WorkspaceViewModel()
        {
            _items.Add(new AddTrackPlaceholder());
        }

        private void OpenAddTrackDialog()
        {
            var newTrack = new TrackViewModel(new Track
            {
                Name = "New Track",
                BorderColor = System.Windows.Media.Colors.Red,
                PlayMode = PlayMode.Continuous,
                QueueMode = QueueMode.Shuffle
            });

            // Insert before the placeholder
            _items.Insert(_items.Count - 1, newTrack);
        }
    }
}