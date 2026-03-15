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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Soundbard.Models;
using Soundbard.ViewModels;

namespace Soundbard.Components
{
    /// <summary>
    /// Interaction logic for TrackCard.xaml
    /// </summary>
    public partial class TrackCard : UserControl
    {
        public TrackCard()
        {
            InitializeComponent();
            DataContext = new TrackViewModel(new Track
            {
                Name = "Battle Music",
                BorderColor = Colors.Red,
                PlayMode = PlayMode.Continuous,
                QueueMode = QueueMode.Shuffle,
                FadeEnabled = true,
                FadeDuration = 2
            });

        }
    }
}
