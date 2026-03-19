using Soundbard.Commands;
using Soundbard.Models;
using Soundbard.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Soundbard.ViewModels
{
    public class WorkspaceViewModel : BaseViewModel
    {
        private readonly ObservableCollection<object> _items = new();
        public ObservableCollection<object> Items => _items;

        public ICommand AddTrackCommand => new RelayCommand(OpenTrackOptionsDialog);

        public WorkspaceViewModel()
        {
            _items.Add(new AddTrackPlaceholder());
        }

        public void DeleteTrack(TrackViewModel track)
        {
            _items.Remove(track);
        }

        private void OpenTrackOptionsDialog()
        {
            var dialog = new TrackOptionsDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.Result != null)
                _items.Insert(_items.Count - 1, new TrackViewModel(dialog.Result, this));
        }
    }
}