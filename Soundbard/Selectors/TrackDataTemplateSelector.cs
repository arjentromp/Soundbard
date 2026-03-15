using System;
using System.Collections.Generic;
using System.Text;
using Soundbard.Models;
using Soundbard.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Soundbard.Components
{
    public class TrackDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TrackTemplate { get; set; }
        public DataTemplate? AddButtonTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item is AddTrackPlaceholder ? AddButtonTemplate : TrackTemplate;
        }
    }
}