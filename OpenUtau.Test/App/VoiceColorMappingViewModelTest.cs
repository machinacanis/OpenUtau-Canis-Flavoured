using OpenUtau.App.ViewModels;
using Xunit;

namespace OpenUtau.App {
    public class VoiceColorMappingViewModelTest {
        [Fact]
        public void Ctor_EmptyNewColors_UsesDefaultSlot() {
            var vm = new VoiceColorMappingViewModel(new[] { "", "Power" }, System.Array.Empty<string>(), "Track1");

            Assert.Equal("Track1", vm.TrackName);
            Assert.Equal(2, vm.ColorMappings.Count);
            Assert.Equal("(Default)", vm.ColorMappings[0].NewColors[0]);
            Assert.Equal(0, vm.ColorMappings[1].SelectedIndex);
        }
    }
}
