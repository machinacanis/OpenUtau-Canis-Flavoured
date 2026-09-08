namespace OpenUtau.App.Studio {
    /// <summary>
    /// Studio-only track header metrics. Classic values stay in
    /// <see cref="ViewConstants"/>; Studio starts one compact step below them.
    /// </summary>
    public static class StudioTrackLayout {
        /// <summary>
        /// 105 − 2 × 21 = 63. A single step (84) still shows the phonemizer row,
        /// which the compact Studio header drops.
        /// </summary>
        public const double CompactTrackHeight =
            ViewConstants.TrackHeightDefault - 2 * ViewConstants.TrackHeightDelta;

        /// <summary>Track height used when a session starts in this mode.</summary>
        public static double DefaultTrackHeight =>
            StudioUI.IsEnabled ? CompactTrackHeight : ViewConstants.TrackHeightDefault;

        /// <summary>Track color bar width; 0 keeps the classic header.</summary>
        public static double ColorBarWidth => StudioUI.IsEnabled ? 6 : 0;
    }
}
