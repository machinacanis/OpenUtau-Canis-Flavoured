using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;

namespace OpenUtau.App.ViewModels {
    public class VoiceColorMappingViewModel : ViewModelBase {

        public string TrackName { get; set; }
        public ObservableCollection<ColorMapping> ColorMappings { get; set; } = new ObservableCollection<ColorMapping>();

        public VoiceColorMappingViewModel(string[] oldColors, string[] newColors, string trackName) {
            oldColors ??= Array.Empty<string>();
            var colors = newColors ?? Array.Empty<string>();
            var NewColors = new ObservableCollection<string>(colors);
            if (NewColors.Count == 0) {
                NewColors.Add("(Default)");
            } else {
                NewColors[0] = "(Default)";
            }
            TrackName = trackName;
            for (int i = 0; i < oldColors.Length; i++) {
                if (i == 0) {
                    ColorMappings.Add(new ColorMapping("(Default)", 0, 0, NewColors));
                } else if (colors.Contains(oldColors[i])) {
                    ColorMappings.Add(new ColorMapping(oldColors[i], i, Array.IndexOf(colors, oldColors[i]), NewColors));
                } else if (i < colors.Length) {
                    ColorMappings.Add(new ColorMapping(oldColors[i], i, i, NewColors));
                } else {
                    ColorMappings.Add(new ColorMapping(oldColors[i], i, 0, NewColors));
                }
            }
        }
    }

    public class ColorMapping {
        public string Name { get; set; }
        public int OldIndex { get; set; }
        public int SelectedIndex { get; set; }
        public ObservableCollection<string> NewColors { get; set; }

        public ColorMapping(string name,int oldIndex, int selectedIndex, ObservableCollection<string> newColors) {
            Name = name;
            OldIndex = oldIndex;
            SelectedIndex = selectedIndex;
            NewColors = newColors;
        }
    }
}
