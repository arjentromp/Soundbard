using Soundbard.Commands;
using Soundbard.Models;
using Soundbard.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Soundbard.ViewModels
{
    public class WorkspaceViewModel : BaseViewModel
    {
        private readonly ObservableCollection<object> _items = new();
        public ObservableCollection<object> Items => _items;

        public ICommand AddTrackCommand => new RelayCommand(OpenTrackOptionsDialog);

        public string ProfilesDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SoundBard",
            "Profiles");

        public WorkspaceViewModel()
        {
            _items.Add(new AddTrackPlaceholder());
        }

        public void DeleteTrack(TrackViewModel track)
        {
            _items.Remove(track);
        }

        public int LoadTracks(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new ColorJsonConverter());
            List<Track?>? loadedTracks = JsonSerializer.Deserialize<List<Track?>>(stream, options);
            if (loadedTracks == null || loadedTracks.Any(track => track == null))
                throw new InvalidDataException("The selected file does not contain a valid track profile.");

            var tracks = loadedTracks.Cast<Track>().ToList();
            if (tracks.Any(track => track.Name == null || track.Hotkey == null || track.FilePaths == null || track.FilePaths.Any(path => path == null)))
                throw new InvalidDataException("The selected profile contains invalid track data.");

            var newTrackViewModels = tracks.Select(track => new TrackViewModel(track, this)).ToList();
            var oldTrackViewModels = _items.OfType<TrackViewModel>().ToList();
            foreach (var track in oldTrackViewModels)
                track.DisposeAudioService();

            _items.Clear();
            foreach (var track in newTrackViewModels)
                _items.Add(track);
            _items.Add(new AddTrackPlaceholder());
            return newTrackViewModels.Count;
        }

        public string SaveTracks(string saveName)
        {
            if (string.IsNullOrWhiteSpace(saveName) ||
                saveName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                saveName is "." or "..")
            {
                throw new ArgumentException("A valid save file name is required.", nameof(saveName));
            }

            Directory.CreateDirectory(ProfilesDirectory);

            string filePath = Path.Combine(ProfilesDirectory, $"{saveName}.json");
            var tracks = _items
                .OfType<TrackViewModel>()
                .Select(track => track.GetModel())
                .ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new ColorJsonConverter());

            using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
            JsonSerializer.Serialize(stream, tracks, options);
            return filePath;
        }

        private void OpenTrackOptionsDialog()
        {
            var dialog = new TrackOptionsDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.Result != null)
                _items.Insert(_items.Count - 1, new TrackViewModel(dialog.Result, this));
        }

        private sealed class ColorJsonConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string value = reader.GetString() ?? string.Empty;
                    if (value.Length == 7 && value[0] == '#')
                        return Color.FromRgb(ParseByte(value.AsSpan(1, 2)), ParseByte(value.AsSpan(3, 2)), ParseByte(value.AsSpan(5, 2)));
                    if (value.Length == 9 && value[0] == '#')
                        return Color.FromArgb(ParseByte(value.AsSpan(1, 2)), ParseByte(value.AsSpan(3, 2)), ParseByte(value.AsSpan(5, 2)), ParseByte(value.AsSpan(7, 2)));

                    throw new JsonException("The profile contains an invalid color value.");
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    using JsonDocument document = JsonDocument.ParseValue(ref reader);
                    JsonElement color = document.RootElement;
                    if (TryGetByte(color, "A", out byte alpha) &&
                        TryGetByte(color, "R", out byte red) &&
                        TryGetByte(color, "G", out byte green) &&
                        TryGetByte(color, "B", out byte blue))
                    {
                        return Color.FromArgb(alpha, red, green, blue);
                    }
                }

                throw new JsonException("The profile contains an invalid color value.");
            }

            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            {
                writer.WriteStringValue($"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}");
            }

            private static bool TryGetByte(JsonElement element, string propertyName, out byte value)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        return property.Value.TryGetByte(out value);
                }

                value = 0;
                return false;
            }

            private static byte ParseByte(ReadOnlySpan<char> value)
            {
                if (byte.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte result))
                    return result;

                throw new JsonException("The profile contains an invalid color value.");
            }
        }
    }
}